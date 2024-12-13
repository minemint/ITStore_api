using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace ITStoreApi
{
    // สร้างคลาส SqlDataAccess สำหรับการเข้าถึงฐานข้อมูล
    public class SqlDataAccess
    {
        // ประกาศตัวแปร _config เพื่อเก็บการตั้งค่าคอนฟิก
        public IConfiguration _config;

        // ประกาศตัวแปร ConnectionStringName เพื่อเก็บชื่อของ Connection String และกำหนดค่าเริ่มต้นเป็น "Default"
        public string ConnectionStringName { get; set; } = "Default";

        // คอนสตรัคเตอร์ของคลาส SqlDataAccess รับพารามิเตอร์เป็น IConfiguration และกำหนดค่าให้กับตัวแปร _config
        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
        }

        // ฟังก์ชันสำหรับดึงข้อมูลจากฐานข้อมูล
        public async Task<List<T>> LoadData<T, U>(string sql, U parameter)
        {
            // ดึง Connection String จากการตั้งค่า
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            // สร้างการเชื่อมต่อกับฐานข้อมูล
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                // รันคำสั่ง SQL และดึงข้อมูล
                var data = await connection.QueryAsync<T>(sql, parameter);

                // แปลงข้อมูลเป็น List และส่งกลับ
                return data.ToList();
            }
        }

        // ฟังก์ชันสำหรับบันทึกข้อมูลลงฐานข้อมูล
        public async Task SaveData<T>(string sql, T parameter)
        {
            // ดึง Connection String จากการตั้งค่า
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            // สร้างการเชื่อมต่อกับฐานข้อมูล
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                // รันคำสั่ง SQL เพื่อบันทึกข้อมูล
                await connection.ExecuteAsync(sql, parameter);
            }
        }

        // ฟังก์ชันสำหรับดึงข้อมูลเดี่ยวจากฐานข้อมูล
        public async Task<T> LoadSingleData<T, U>(string sql, U parameter)
        {
            // ดึง Connection String จากการตั้งค่า
            string connectionString = _config.GetConnectionString(ConnectionStringName);

            // สร้างการเชื่อมต่อกับฐานข้อมูล
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                // รันคำสั่ง SQL และดึงข้อมูลเดี่ยว
                var data = await connection.QueryFirstOrDefaultAsync<T>(sql, parameter);

                // ส่งข้อมูลกลับ
                return data;
            }
        }
    }
}
