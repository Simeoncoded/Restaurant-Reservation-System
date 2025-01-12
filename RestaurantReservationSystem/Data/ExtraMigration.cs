using Microsoft.EntityFrameworkCore.Migrations;

namespace RestaurantReservationSystem.Data
{
    public static class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
            // Drop triggers if they already exist
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS SetReservationTimestampOnUpdate;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS SetReservationTimestampOnInsert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS SetTableTimestampOnUpdate;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS SetTableTimestampOnInsert;");

            // Create triggers for Reservations table
            migrationBuilder.Sql(
                @"
                CREATE TRIGGER SetReservationTimestampOnUpdate
                AFTER UPDATE ON Reservations
                BEGIN
                    UPDATE Reservations
                    SET RowVersion = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ");

            migrationBuilder.Sql(
                @"
                CREATE TRIGGER SetReservationTimestampOnInsert
                AFTER INSERT ON Reservations
                BEGIN
                    UPDATE Reservations
                    SET RowVersion = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ");

            // Create triggers for Tables table
            migrationBuilder.Sql(
               @"
                CREATE TRIGGER SetTableTimestampOnUpdate
                AFTER UPDATE ON Tables
                BEGIN
                    UPDATE Tables
                    SET RowVersion = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ");

            migrationBuilder.Sql(
                @"
                CREATE TRIGGER SetTableTimestampOnInsert
                AFTER INSERT ON Tables
                BEGIN
                    UPDATE Tables
                    SET RowVersion = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ");
        }
    }
}
