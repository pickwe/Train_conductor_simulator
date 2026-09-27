# Package

version       = "0.1.0"
author        = "tolant_team"
description   = "A server"
license       = "NO"
srcDir        = "src"
bin           = @["server/server"]


requires "nim >= 2.2.10"
requires "jsony"
requires "db_connector"
requires "debby"
requires "checksums"
requires "https://github.com/haxscramper/hmatching"
