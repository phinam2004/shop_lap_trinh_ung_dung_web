using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020673.DataLayers.Interfaces;
using SV22T1020673.Models.Security;
using System.Data;

namespace SV22T1020673.DataLayers.SQLServer
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly string connectionString;

        public UserAccountRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private IDbConnection OpenConnection()
        {
            IDbConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"
                    SELECT TOP (1)
                        EmployeeID AS UserId,
                        Email AS UserName,
                        FullName AS DisplayName,
                        Email,
                        Photo,
                        RoleNames
                    FROM Employees
                    WHERE Email = @userName
                      AND [Password] = @password
                      AND IsWorking = 1;
                ";

                return await connection.QueryFirstOrDefaultAsync<UserAccount>(sql, new { userName, password });
            }
        }

        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using (var connection = OpenConnection())
            {
                var sql = @"
                    UPDATE Employees
                    SET [Password] = @password
                    WHERE Email = @userName;
                ";

                int rows = await connection.ExecuteAsync(sql, new { userName, password });
                return rows > 0;
            }
        }
    }
}
