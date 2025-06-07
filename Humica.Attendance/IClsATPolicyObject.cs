using Humica.Core.DB;
using Humica.Core.FT;
using Humica.EF;
using Humica.EF.MD;
using System;
using System.Collections.Generic;

namespace Humica.Attendance
{
    public interface IClsATPolicyObject : IClsApplication
    {
        ATPolicy Header { get; set; }
        string ScreenId { get; set; }

        void OnIndexLoading();
        string Update();
        //FTFilterAttenadance Attenadance { get; set; }
        //List<VIEW_ATEmpSchedule> ListEmpSchdule { get; set; }
        //HRStaffProfile StaffProfile { get; set; }
        //void OnIndexLoading();
        //void OnFilterStaff(string EmpCode);
        //string GenrateAttendance(string TranNo);

        //Dictionary<string, dynamic> OnDataSelector(params object[] keys);
        //string Set_DefaultShift(DateTime FromDate, DateTime ToDate, List<HRBranch> ListBranch);
    }
}
