import pkg/debby/sqlite
export sqlite

type
  Account* = ref object
    id*: int
    token*: string
    name*: string
    login*: string
    passwordHash*: string
  
  ActionOutcome* = ref object
    id*: int
    userId*: int
    reason*: string
    safety*: int
    loyality*: int

const databaseFile = "db.sqlite"


proc openDatabase*(): Db =
  openDatabase(databaseFile)


proc setupTables*(db: Db) =
  if not db.tableExists(Account): db.createTable(Account)
  if not db.tableExists(ActionOutcome): db.createTable(ActionOutcome)

