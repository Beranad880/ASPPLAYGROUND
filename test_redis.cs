using System;
using StackExchange.Redis;

class Program {
    static void Main() {
        try {
            var url = "redis://default:my_pass_123@redis.railway.internal:6379";
            var opts = ConfigurationOptions.Parse(url);
            Console.WriteLine("Success: " + opts.EndPoints[0] + " pwd: " + opts.Password);
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
