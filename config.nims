
--define:ssl


task runServer, "build and run server":
  exec "nim c -r server/server.nim"

task buildServer, "build and run server":
  exec "nim c server/server.nim"
