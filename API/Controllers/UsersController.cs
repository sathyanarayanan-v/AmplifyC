using System;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class UsersController : BaseController
    {
        private readonly DataContext _context;
        public UsersController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<AppUser>> GetUsers()
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.UserName == x.UserName.ToLower());

        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUsersById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }
    }
}