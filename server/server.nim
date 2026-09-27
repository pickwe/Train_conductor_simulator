import std/[asynchttpserver, asyncdispatch, strutils, logging, os, oids]
import pkg/checksums/sha1
import pkg/[hmatching, jsony]
import ./[httputils, application, database]

let salt = readFile("salt.txt")


proc authorizeUser(db: Db, token: string): int =
  let users = db.filter(Account, it.token == token)
  if users.len != 0:
    users[0].id
  else:
    raise UnauthorizedUserError.newException("unauthorized")


proc userApi: seq[Route] =
  result.add: route HttpPost, ["login"]:
    type Body = object
      login, password: string
    let body = req.body.fromJson(Body)
    
    let passwordHash = $secureHash(salt & body.password)
    let users = app.db.filter(Account, it.login == body.login and it.passwordHash == passwordHash)
    if users.len == 0:
      await req.respond(Http403, "wrong credentials"); return
    else:
      await req.respond(Http200, users[0].token); return
  

  result.add: route HttpPost, ["register"]:
    type Body = object
      login, password: string
      name: string
    let body = req.body.fromJson(Body)

    if body.name == "" or body.login == "" or body.password == "":
      await req.respond(Http400, "wrong data schema"); return
    
    if app.db.filter(Account, it.login == body.login).len != 0:
      await req.respond(Http403, "user with this login already exists"); return

    let user = Account(
      token: $genOid(),
      name: body.name,
      login: body.login,
      passwordHash: $secureHash(salt & body.password)
    )
    app.db.insert(user)

    await req.respond(Http200, user.token); return


  result.add: route HttpPost, ["actOutcome"]:
    type Body = object
      reason: string
      safety, loyality: int
    let body = req.body.fromJson(Body)

    let userId = app.db.authorizeUser(req.headers["token"])
    app.db.insert ActionOutcome(
      userId: userId,
      reason: body.reason,
      safety: body.safety,
      loyality: body.loyality,
    )
    await req.respond(Http200, "ok")


  result.add: route HttpGet, ["stats"]:
    let userId = app.db.authorizeUser(req.headers["token"])
    type Response = object
      safety, loyality: int
    
    var res = Response(safety: 70, loyality: 70)
    for x in app.db.filter(ActionOutcome, it.userId == userId):
      res.safety += x.safety
      res.loyality += x.loyality
    
    await req.respond(Http200, res.toJson, json_headers)


  # result.add: route HttpGet, ["leaderboard"]:
  #   let params = req.params
    
  #   params.withPaging(page, limit):
  #     await req.respond(Http200, $ %*app.getModelsPage(page, limit), json_headers)



when isMainModule:
  addHandler newConsoleLogger(fmtStr = "[$date at $time]:$levelname ")
  let app = newApplication(openDatabase())
  app.db.setupTables()

  let staticFilesRoot =
    if dirExists(getCurrentDir() / "server" / "public"): getCurrentDir() / "server" / "public"
    elif dirExists(getCurrentDir() / "public"): getCurrentDir() / "public"
    else: raise ValueError.newException("please make sure you ran server either near server/public/ or near public/")

  app.add staticRoute("/", "", staticFilesRoot/"html/index.html", html_headers)
  app.add staticRoute("/", "", staticFilesRoot/"html/", html_headers)
  app.add staticRoute("/", ".html", staticFilesRoot/"html/", html_headers)
  app.add staticRoute("/", ".js", staticFilesRoot/"js/", js_headers)
  app.add staticRoute("/", ".css", staticFilesRoot/"css/", css_headers)

  app.add userApi()

  waitFor run app
