﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Humica.Training.DB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class TrainingExamItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int LineItem { get; set; }
        public string ExamCode { get; set; }
        public string StaffID { get; set; }
        public string AnswerCode { get; set; }
        public string Topic { get; set; }
        public string Coursecode { get; set; }
        public string TrainingType { get; set; }
        public string QuestionCode { get; set; }
        public string InvCode { get; set; }
        public Nullable<System.DateTime> ExamDate { get; set; }
        public decimal TotalScore { get; set; }
        public string QuestionDescription { get; set; }
        public string AnswerDescription { get; set; }
        public int CheckValue { get; set; }
        public int ColumnCheck { get; set; }
        public int CorrectValue { get; set; }
    }
}
