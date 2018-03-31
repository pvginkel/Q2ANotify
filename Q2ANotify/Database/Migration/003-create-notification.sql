CREATE TABLE "notification" (
	"userid" INT NOT NULL,
	"datetime" INT NOT NULL,
	"kind" TEXT NOT NULL,
	"user" TEXT NULL,
	"poster" TEXT NULL,
	"title" TEXT NOT NULL,
	"message" TEXT NULL,
	"parentid" INT NULL,
	"postid" INT NULL
);

CREATE INDEX "idx_notification_userid_datetime" ON "notification" ("userid", "datetime");
