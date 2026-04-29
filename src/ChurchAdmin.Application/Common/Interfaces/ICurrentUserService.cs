using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchAdmin.Application.Common.Interfaces;
public interface ICurrentUserService
{
    string UserId { get; }

    string Email { get; }
}
