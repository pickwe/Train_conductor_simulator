import std/[asynchttpserver, asyncdispatch, strutils, sequtils, logging, uri, os]
import ./[httputils, database]


type
  UnauthorizedUserError* = object of CatchableError

  Route* = proc(app: Application, req: Request, path: seq[string]): Future[bool] {.async, gcsafe.}

  Application* = ref object
    db*: Db
    server: AsyncHttpServer
    routes: seq[Route]
    exit*: bool


proc newApplication*(database: Db, port = 5416.Port): Application =
  result = Application(db: database)
  let app = result

  app.server = newAsyncHttpServer()
  app.server.listen(port)


proc add*(app: Application, route: varargs[Route]) =
  app.routes.add route


proc run*(app: Application) {.async.} =
  echo "listening on http://localhost:" & $app.server.getPort.uint16 & "/"

  proc route(req: Request) {.async, gcsafe.} =
    info("request: " & $req.url & " [" & $req.reqMethod & "]")
    let pathSeq = req.url.path.split("/").filterIt(it != "")

    for route in app.routes:
      if route(app, req, pathSeq).await: return
    
    await req.respond(Http404, "Not found")
  
  while true:
    if app.exit: break
    if app.server.shouldAcceptRequest():
      await app.server.acceptRequest(wrapErrorHandling(route))
    else:
      await sleepAsync(10)



proc staticRoute*(reqPath: string, reqExt: string, dirPath: string, headers: HttpHeaders): Route =
  proc(app: Application, req: Request, pathSeq: seq[string]): Future[bool] {.async, gcsafe.} =
    let (path, filename, ext) = req.url.path.splitFile
    if path == reqPath and ext == reqExt:
      try:
        await req.respond(Http200, readFile(dirPath / (filename & ext)), headers); return true
      except:
        return false


template route*(meth: HttpMethod, reqQuery: untyped, body: untyped): Route =
  proc(
    app {.inject.}: Application, req {.inject.}: Request, path {.inject.}: seq[string]
  ): Future[bool] {.async, gcsafe.} =
    {.cast(gcsafe).}:
      if req.reqMethod != meth: return false
      case path
      of reqQuery:
        result = true
        try:
          body
        except UnauthorizedUserError:
          await req.respond(Http403, "unauthorized")
        except:
          await req.respond(Http500, "error")
      else:
        return false


