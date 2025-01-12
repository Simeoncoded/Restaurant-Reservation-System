using Microsoft.EntityFrameworkCore.Migrations;

namespace RestaurantReservationSystem.Data
{
    public static class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
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
        }
    }

}
