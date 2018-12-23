using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using DateCore.API.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace DateCore.API.Data
{
    public class Seed
    {
        private readonly UserManager<User> _userManager;
        public Seed(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public void SeedUsers()
        {
            if(!_userManager.Users.Any())
            {
                var userData = File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);
                foreach (var user in users)
                {
                    _userManager.CreateAsync(user, "password").Wait();
                }
            }
        }
    }
}