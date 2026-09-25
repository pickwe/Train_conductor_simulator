import pkg/debby/sqlite
export sqlite

type
  Account* = ref object
    id*: int
    token*: string
    name*: string
    login*: string
    passwordHash*: string

const databaseFile = "db.sqlite"


proc openDatabase*(): Db =
  openDatabase(databaseFile)


proc setupTables*(db: Db) =
  if not db.tableExists(Account): db.createTable(Account)

