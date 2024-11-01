using System;
using SampleSecureWeb.Models;

namespace SampleSecureWeb.Data;

public interface IUser
{
    User Registration(User user);
    User Login(User user);
    User GetCurrentUser(string username);
    void UpdatePassword(User user);
}
