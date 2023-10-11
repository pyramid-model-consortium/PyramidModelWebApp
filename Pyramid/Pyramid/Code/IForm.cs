using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pyramid.Code
{
    interface IForm
    {
        string FormAbbreviation
        {
            get;
        }

        Models.CodeProgramRolePermission FormPermissions
        {
            get;
            set;
        }
    }
}
