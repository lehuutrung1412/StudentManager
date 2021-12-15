﻿using StudentManagement.Models;
using StudentManagement.Objects;
using StudentManagement.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Services
{
    public class CourseRegisterServices
    {
        private static CourseRegisterServices s_instance;

        public static CourseRegisterServices Instance => s_instance ?? (s_instance = new CourseRegisterServices());

        public CourseRegisterServices() { }

        public CourseRegister GetFirstCourseRegister()
        {
            return DataProvider.Instance.Database.CourseRegisters.FirstOrDefault();
        }

        public ObservableCollection<SubjectClass> LoadCourseRegisteredListByStudentId(Guid idStudent)
        {
            ObservableCollection<SubjectClass> listSubjectClass = new ObservableCollection<SubjectClass>();
            List<CourseRegister> listCourseRegistered = DataProvider.Instance.Database.CourseRegisters.Where(y => y.IdStudent == idStudent).Where(z => z.Status == 1).ToList();
            foreach (CourseRegister registeredCourse in listCourseRegistered)
            {
                listSubjectClass.Add(registeredCourse.SubjectClass);
            }
            return listSubjectClass;
        }
        public ObservableCollection<SubjectClass> LoadCourseRegisteredListBySemesterIdAndStudentId(Guid idSemester, Guid idStudent)
        {
            ObservableCollection<SubjectClass> listSubjectClass = new ObservableCollection<SubjectClass>();
            List<CourseRegister> listCourseRegistered = DataProvider.Instance.Database.CourseRegisters.Where(x => x.SubjectClass.IdSemester == idSemester).Where(y => y.IdStudent == idStudent).Where(z => z.Status == 1).ToList();
            foreach (CourseRegister registeredCourse in listCourseRegistered)
            {
                listSubjectClass.Add(registeredCourse.SubjectClass);
            }
            return listSubjectClass;
        }
        public ObservableCollection<SubjectClass> LoadCourseUnregisteredListBySemesterIdAndStudentId(Guid idSemester, Guid idStudent)
        {
            ObservableCollection<SubjectClass> listSubjectClassInSemester = new ObservableCollection<SubjectClass>(SubjectClassServices.Instance.LoadSubjectClassListBySemesterId(idSemester));
            ObservableCollection<SubjectClass> listSubjectClassRegistered = LoadCourseRegisteredListBySemesterIdAndStudentId(idSemester, idStudent);
            foreach (SubjectClass registered in listSubjectClassRegistered)
            {
                listSubjectClassInSemester.Remove(registered);
            }
            return listSubjectClassInSemester;
        }

        public CourseRegister FindCourseRegisterBySemesterIdAndStudentIdAndSubjectClassId(Guid idSemester, Guid idStudent, Guid idSubjectClass)
        {
            return DataProvider.Instance.Database.CourseRegisters
                .Where(register => register.SubjectClass.IdSemester == idSemester)
                .Where(register => register.IdStudent == idStudent)
                .Where(register => register.IdSubjectClass == idSubjectClass)
                .FirstOrDefault();
        }
        public bool StudentRegisterSubjectClassToDatabase(Guid idSemester, Guid idStudent, SubjectClass subjectClass)
        {
            try
            {
                CourseRegister registering = new CourseRegister()
                {
                    Id = Guid.NewGuid(),
                    IdStudent = idStudent,
                    IdSubjectClass = subjectClass.Id,
                    Status = 1,
                };
                DataProvider.Instance.Database.CourseRegisters.Add(registering);
                DataProvider.Instance.Database.SaveChanges();

                return true;
            }
            catch (DbUpdateException e)
            {
                MyMessageBox.Show("Lớp học đã đủ số lượng, đăng ký không thành công");
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool StudentUnregisterSubjectClassToDatabase(Guid idSemester, Guid idStudent, SubjectClass subjectClass)
        {
            CourseRegister registered = FindCourseRegisterBySemesterIdAndStudentIdAndSubjectClassId(idSemester, idStudent, subjectClass.Id);
            DataProvider.Instance.Database.CourseRegisters.Remove(registered);
            DataProvider.Instance.Database.SaveChanges();
            return true;
        }

        public List<Student> FindStudentsBySubjectClassId(Guid idSubjectClass)
        {
            return DataProvider.Instance.Database.CourseRegisters
                .Where(register => register.IdSubjectClass == idSubjectClass)
                .Select(student => student.Student).ToList();
        }

        public CourseRegister FindCourseRegisterByStudentIdAndSubjectClassId(Guid idStudent, Guid idSubjectClass)
        {
            return DataProvider.Instance.Database.CourseRegisters
                .Where(register => register.IdStudent == idStudent)
                .Where(register => register.IdSubjectClass == idSubjectClass)
                .FirstOrDefault();
        }

        public async Task StudentUnregisterSubjectClassDetailToDatabase(Guid idStudent, SubjectClass subjectClass)
        {
            CourseRegister registered = FindCourseRegisterByStudentIdAndSubjectClassId(idStudent, subjectClass.Id);
            DataProvider.Instance.Database.CourseRegisters.Remove(registered);
            await DataProvider.Instance.Database.SaveChangesAsync();
        }

        public async Task StudentRegisterSubjectClassDetailToDatabase(Guid idStudent, SubjectClass subjectClass)
        {
            CourseRegister registering = new CourseRegister()
            {
                Id = Guid.NewGuid(),
                IdStudent = idStudent,
                IdSubjectClass = subjectClass.Id,
                Status = 1,
            };
            DataProvider.Instance.Database.CourseRegisters.AddOrUpdate(registering);
            await DataProvider.Instance.Database.SaveChangesAsync();
        }

    }
}
