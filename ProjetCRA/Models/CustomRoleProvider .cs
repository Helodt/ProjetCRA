using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace ProjetCRA.Models
{
	public class CustomRoleProvider : RoleProvider
	{

		BD_CRAEntities db = new BD_CRAEntities();

		public CustomRoleProvider() { }

		public override bool IsUserInRole(string username, string roleName)
		{

			var result = from u in db.UTILISATEUR
						 where u.MATRICULE == username
						 select new { isAdmin = u.ISADMIN };
			var t = result.FirstOrDefault();

			if (roleName == "Admin")
            {
				return t.isAdmin;
            }
			else
            {
				return !t.isAdmin;
			}

		}

		public override string[] GetRolesForUser(string username)
		{

			var result = from u in db.UTILISATEUR
						 where u.MATRICULE == username
						 select new { isAdmin = u.ISADMIN };
			var t = result.FirstOrDefault();

			if (t.isAdmin == true)
            {
				return new string[] { "Admin" };
            }
			else
            {
				return new string[] { "User" };
			}

			// -----------------------------------------
			/*if (username == "admin")
            {
				return new string[] { "Admin" };
            } else
            {
				return new string[] { "User" };
            }*/

		}

		#region Not Implemented Methods

		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public override string ApplicationName
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override void CreateRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			throw new NotImplementedException();
		}

		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			throw new NotImplementedException();
		}

		public override string[] GetAllRoles()
		{
			throw new NotImplementedException();
		}

		public override string[] GetUsersInRole(string roleName)
		{
			throw new NotImplementedException();
		}

		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			throw new NotImplementedException();
		}

		public override bool RoleExists(string roleName)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}