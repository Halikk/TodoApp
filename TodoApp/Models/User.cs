using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace TodoApp.Models
{
    // IdentityUser<int> sınıfından türeyen User sınıfı
    public class User : IdentityUser<int>
    {
        // Kullanıcının görevleri
        public virtual ICollection<TodoItem>? TodoItems { get; set; }
    }
}
