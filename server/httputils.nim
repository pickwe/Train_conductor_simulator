import std/[asynchttpserver, strutils, sequtils, logging, asyncdispatch, uri, asyncfile, tables]


template json_headers*: HttpHeaders = {"Content-type": "application/json; charset=utf-8"}.newHttpHeaders()
template js_headers*: HttpHeaders = {"Content-type": "text/javascript; charset=utf-8"}.newHttpHeaders()
template css_headers*: HttpHeaders = {"Content-type": "text/css; charset=utf-8"}.newHttpHeaders()
template html_headers*: HttpHeaders = {"Content-type": "text/html; charset=utf-8"}.newHttpHeaders()
template plain_headers*: HttpHeaders = {"Content-type": "text/plain; charset=utf-8"}.newHttpHeaders()


proc logException*(e: ref Exception, url: string) =
  var msg = e.msg
  var isTraceAsync = false
  var trace: string = ""

  proc filterProc(s: string): bool =
    s.startsWith("  ") and
    "cligen" notin s and
    "asyncdispatch.nim" notin s and
    "asynchttpserver.nim" notin s and
    "threadimpl.nim" notin s and
    "asyncmacro.nim" notin s and
    "asyncfutures.nim" notin s

  isTraceAsync = "Async traceback:" in msg
  if isTraceAsync:
    trace = msg.splitLines.filter(filterProc).join("\n").dedent(2)
    msg = msg[0 .. msg.find("Async traceback:") - 2]

  error(
    (if url != "": url & ": " else: "") &
    msg &
    " [" & $e.name & "]" &
    "\n" & ("Async traceback (most recent call last)\n" & trace).indent(2)
  )


proc wrapErrorHandling*(
  router: proc (request: Request) {.closure, async, gcsafe.}
): proc (request: Request) {.closure, async, gcsafe.} =
  proc(request: Request) {.closure, async, gcsafe.} =
    var reqh: Future[void]

    try:
      reqh = router(request)
      await reqh

    except Defect:
      logException(reqh.error, $request.url)
      try:
        await request.respond(Http500, "Внутренняя ошибка сервера", plain_headers)
      except: discard

    except Exception:
      logException(reqh.error, $request.url)
      try:
        await request.respond(Http500, "Внутренняя ошибка сервера", plain_headers)
      except: discard



proc asyncReadFile*(name: string, mode: FileMode = fmRead): Future[string] {.async.} =
  let f = openAsync(name, mode)
  result = f.readAll.await
  close f


proc params*(req: Request): Table[string, string] =
  req.url.query.decodeQuery.toSeq.toTable()
    

template withPaging*(params: Table[string, string], page, limit, body) =
  let page =
    try: parseInt(params.getOrDefault("page", "1"))
    except: await req.respond(Http400, "page must be number", plain_headers); return
  
  let limit =
    try: parseInt(params.getOrDefault("limit", "10"))
    except: await req.respond(Http400, "limit must be number", plain_headers); return
  if limit notin 1..100:
    await req.respond(Http400, "limit must be 1..100", plain_headers); return
  
  body


