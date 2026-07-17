using System;
using BCrypt.Net;

namespace HashGen {
    class Program {
        static void Main() {
            Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("vizitlink3d2024"));
        }
    }
}
