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

  AchievementUnlock* = ref object
    id*: int
    userId*: int
    achievementId*: string

  ScenarioCompletion* = ref object
    id*: int
    userId*: int
    runId*: string
    scenarioId*: string

const databaseFile = "db.sqlite"


proc openDatabase*(): Db =
  openDatabase(databaseFile)


proc setupTables*(db: Db) =
  if not db.tableExists(Account): db.createTable(Account)
  if not db.tableExists(ActionOutcome): db.createTable(ActionOutcome)
  if not db.tableExists(AchievementUnlock): db.createTable(AchievementUnlock)
  if not db.tableExists(ScenarioCompletion): db.createTable(ScenarioCompletion)

