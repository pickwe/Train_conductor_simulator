import std/[algorithm, asynchttpserver, asyncdispatch, strutils, logging, os, oids]
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


const
  RegularAchievementExperience = 1000
  HiddenAchievementExperience = 1500
  ScenarioExperience = 500
  ValidAchievementIds = [
    "ACH_PET_01", "ACH_PET_02", "ACH_PET_03", "ACH_PET_04", "ACH_PET_05", "ACH_PET_06",
    "ACH_NOISE_01", "ACH_NOISE_02", "ACH_NOISE_03", "ACH_NOISE_04", "ACH_NOISE_05", "ACH_NOISE_06", "ACH_NOISE_07"
  ]
  HiddenAchievementIds = ["ACH_PET_05", "ACH_PET_06", "ACH_NOISE_05", "ACH_NOISE_07"]


proc isValidAchievement(id: string): bool =
  id in ValidAchievementIds


proc achievementExperience(id: string): int =
  if id in HiddenAchievementIds: HiddenAchievementExperience
  else: RegularAchievementExperience


proc getAchievements(db: Db, userId: int): seq[string] =
  for unlock in db.filter(AchievementUnlock, it.userId == userId):
    result.add unlock.achievementId


proc getExperience(db: Db, userId: int): int =
  for id in db.getAchievements(userId):
    result += achievementExperience(id)
  result += db.filter(ScenarioCompletion, it.userId == userId).len * ScenarioExperience


proc getStats(db: Db, userId: int): tuple[safety, loyality: int] =
  result = (70, 70)
  for outcome in db.filter(ActionOutcome, it.userId == userId):
    result.safety += outcome.safety
    result.loyality += outcome.loyality
  result.safety = max(0, min(100, result.safety))
  result.loyality = max(0, min(100, result.loyality))


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
      name, login: string
      safety, loyality, experience: int
      achievements: seq[string]

    let account = app.db.filter(Account, it.id == userId)[0]
    let stats = app.db.getStats(userId)
    let res = Response(
      name: account.name,
      login: account.login,
      safety: stats.safety,
      loyality: stats.loyality,
      experience: app.db.getExperience(userId),
      achievements: app.db.getAchievements(userId),
    )
    
    await req.respond(Http200, res.toJson, json_headers)


  result.add: route HttpPost, ["achievement"]:
    type Body = object
      id: string
    let body = req.body.fromJson(Body)
    let userId = app.db.authorizeUser(req.headers["token"])
    if not body.id.isValidAchievement:
      await req.respond(Http400, "unknown achievement"); return
    if app.db.filter(AchievementUnlock, it.userId == userId and it.achievementId == body.id).len == 0:
      app.db.insert AchievementUnlock(userId: userId, achievementId: body.id)
    await req.respond(Http200, "ok")


  result.add: route HttpPost, ["scenarioComplete"]:
    type Body = object
      runId, scenarioId: string
    let body = req.body.fromJson(Body)
    let userId = app.db.authorizeUser(req.headers["token"])
    if body.runId == "" or body.scenarioId notin ["pet", "noise"]:
      await req.respond(Http400, "wrong data schema"); return
    if app.db.filter(ScenarioCompletion,
      it.userId == userId and it.runId == body.runId and it.scenarioId == body.scenarioId).len == 0:
      app.db.insert ScenarioCompletion(userId: userId, runId: body.runId, scenarioId: body.scenarioId)
    await req.respond(Http200, "ok")


  result.add: route HttpGet, ["leaderboard"]:
    discard app.db.authorizeUser(req.headers["token"])
    type Entry = object
      name, login: string
      experience, achievements, safety, loyality: int

    var entries: seq[Entry]
    for account in app.db.filter(Account):
      let stats = app.db.getStats(account.id)
      let achievements = app.db.getAchievements(account.id)
      entries.add Entry(
        name: account.name,
        login: account.login,
        experience: app.db.getExperience(account.id),
        achievements: achievements.len,
        safety: stats.safety,
        loyality: stats.loyality,
      )

    entries.sort(proc(a, b: Entry): int =
      result = cmp(b.experience, a.experience)
      if result == 0: result = cmp(a.name, b.name)
    )
    await req.respond(Http200, entries.toJson, json_headers)



when isMainModule:
  addHandler newConsoleLogger(fmtStr = "[$date at $time]:$levelname ")
  let app = newApplication(openDatabase())
  app.db.setupTables()

 

  app.add userApi()

  waitFor run app
