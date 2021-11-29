﻿using ExcelDataReader;
using StudentManagement.Commands;
using StudentManagement.Models;
using StudentManagement.Objects;
using StudentManagement.Services;
using StudentManagement.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using static StudentManagement.ViewModels.StudentCourseRegistryViewModel;

namespace StudentManagement.ViewModels
{
    public class AdminCourseRegistryViewModel : BaseViewModel
    {
        #region classes
        
        #endregion
        #region properties
        private bool _isAllItemsSelected = false;
        public bool IsAllItemsSelected
        {
            get => _isAllItemsSelected;
            set
            {
                _isAllItemsSelected = value;
                OnPropertyChanged();
                CourseRegistryItemsDisplay.Select(c => { c.IsSelected = value; return c; }).ToList();
            }
        }
        private ObservableCollection<Models.SubjectClass> _subjectClasses;
        public ObservableCollection<Models.SubjectClass> SubjectClasses { get => _subjectClasses; set => _subjectClasses = value; }

        private ObservableCollection<ObservableCollection<CourseItems>> _courseRegistryItemsAll;
        public ObservableCollection<ObservableCollection<CourseItems>> CourseRegistryItemsAll { get => _courseRegistryItemsAll; set => _courseRegistryItemsAll = value; }
        private ObservableCollection<CourseItems> _courseRegistryItems;
        public ObservableCollection<CourseItems> CourseRegistryItems { get => _courseRegistryItems; set => _courseRegistryItems = value; }
        private ObservableCollection<CourseItems> _courseRegistryItemsDisplay;
        public ObservableCollection<CourseItems> CourseRegistryItemsDisplay { get => _courseRegistryItemsDisplay; set { _courseRegistryItemsDisplay = value; OnPropertyChanged(); } }

        public ObservableCollection<Models.Semester> Semesters { get => _semesters; set { _semesters = value; OnPropertyChanged(); } }
        private ObservableCollection<Models.Semester> _semesters;

        public Models.Semester SelectedSemester { get => _selectedSemester; 
            set 
            { 
                _selectedSemester = value; 
                OnPropertyChanged(); 
                if(value != null)
                    AdminCourseRegistryRightSideBarViewModel.Instance.CanEdit = !(_selectedSemester.CourseRegisterStatus > 0); 
            } }
        private Models.Semester _selectedSemester;
        public int SelectedSemesterIndex { get => _selectedSemesterIndex; 
            set 
            { 
                _selectedSemesterIndex = value; 
                OnPropertyChanged(); 
                SelectData();

                AdminCourseRegistryRightSideBarViewModel.Instance.RightSideBarItemViewModel = new EmptyStateRightSideBarViewModel();
            } }
        private int _selectedSemesterIndex;

        public VietnameseStringNormalizer vietnameseStringNormalizer = VietnameseStringNormalizer.Instance;
        public bool IsFirstSearchButtonEnabled
        {
            get { return _isFirstSearchButtonEnabled; }
            set
            {
                _isFirstSearchButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isFirstSearchButtonEnabled = false;

        private string _searchQuery = "";
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                SearchCourseRegistryItemsFunction();
            }
        }
        private object _dialogItemViewModel;
        public object DialogItemViewModel
        {
            get { return _dialogItemViewModel; }
            set
            {
                _dialogItemViewModel = value;
                OnPropertyChanged();
            }
        }
        public object _creatNewCourseViewModel;

        #region CreateNewSemester
        private ObservableCollection<string> _batches;
        public ObservableCollection<string> Batches { get => _batches; set { _batches = value; OnPropertyChanged(); } }

        private string _selectedBatch;
        public string SelectedBatch { get => _selectedBatch; set { _selectedBatch = value; OnPropertyChanged(); } }

        private string _newSemesterName;
        public string NewSemesterName { get => _newSemesterName; set { _newSemesterName = value; OnPropertyChanged(); } }

        private bool _isDoneVisible;
        private bool _isErrorVisible;
        public bool IsDoneVisible { get => _isDoneVisible; set { _isDoneVisible = value; OnPropertyChanged(); } }
        public bool IsErrorVisible { get => _isErrorVisible; set { _isErrorVisible = value; OnPropertyChanged(); } }
        #endregion
        #endregion
        #region commands
        public ICommand SwitchSearchButton { get => _switchSearchButton; set => _switchSearchButton = value; }

        private ICommand _switchSearchButton;
        public ICommand SearchCourseRegistryItems { get => _searchCourseRegistryItems; set => _searchCourseRegistryItems = value; }

        private ICommand _searchCourseRegistryItems;
        public ICommand DeleteSelectedItemsCommand { get; set; }
        public ICommand CreateNewCourseCommand { get; set; }

        public ICommand OpenSemesterCommand { get; set; }
        public ICommand PauseSemesterCommand { get; set; }
        public ICommand StopSemesterCommand { get; set; }
        public ICommand CreateNewSemesterCommand { get; set; }
        public ICommand AddFromExcelCommand { get; set; }
        public ICommand SaveChangesCommand { get; set; }
        public ICommand ConvertChangesCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }


        #endregion

        public AdminCourseRegistryViewModel()
        {
            Semesters = SemesterServices.Instance.LoadListSemester();
            SubjectClasses = new ObservableCollection<SubjectClass>(SubjectClassServices.Instance.LoadSubjectClassList());
            CourseRegistryItemsAll = new ObservableCollection<ObservableCollection<CourseItems>>();
            for (int i = 0; i < Semesters.Count; i++)
            {
                Semester semester = Semesters[i];
                var subjectClasses1Semester = SubjectClasses.Where(x => x.Semester == semester).ToList();
                var courseItems1Semester = new ObservableCollection<CourseItems>();
                foreach (Models.SubjectClass a in subjectClasses1Semester)
                {
                    courseItems1Semester.Add(new CourseItems(a, false));
                }
                CourseRegistryItemsAll.Add(courseItems1Semester);
            }
            SelectedSemester = Semesters.LastOrDefault();
            InitCreateNewSemesterProperty();
            InitCommand();
        }

        public void InitCreateNewSemesterProperty()
        {
            NewSemesterName = "Học kỳ 1";

            CreateNewBatch();
            SelectedBatch = Batches.Last();

            IsDoneVisible = false;
            IsErrorVisible = false;
        }
        public void InitCommand()
        {
            SwitchSearchButton = new RelayCommand<object>((p) => { return true; }, (p) => SwitchSearchButtonFunction(p));
            SearchCourseRegistryItems = new RelayCommand<object>((p) => { return true; }, (p) => SearchCourseRegistryItemsFunction());
            DeleteSelectedItemsCommand = new RelayCommand<object>(
                (p) =>
                {
                    return CourseRegistryItemsDisplay.Where(x => x.IsSelected == true).Count() > 0 && !(SelectedSemester.CourseRegisterStatus > 0);
                },
                (p) =>
                {
                    DeleteSelectedItems();
                });
            CreateNewCourseCommand = new RelayCommand<object>((p) => {
                return !(SelectedSemester.CourseRegisterStatus > 0);
            }, (p) => CreateNewCourse());
            OpenSemesterCommand = new RelayCommand<object>((p) => true, (p) => SelectedSemester.CourseRegisterStatus = 1);
            PauseSemesterCommand = new RelayCommand<object>((p) => true, (p) => SelectedSemester.CourseRegisterStatus = 0);
            StopSemesterCommand = new RelayCommand<object>((p) => true, (p) => SelectedSemester.CourseRegisterStatus = 2);

            CreateNewSemesterCommand = new RelayCommand<object>((p) =>
            {
                if (String.IsNullOrEmpty(SelectedBatch) || String.IsNullOrEmpty(NewSemesterName))
                    return false;
                if (Semesters.Where(x => x.Batch.Replace(" ", "") == SelectedBatch.Replace(" ", "")).
                                Where(y => y.DisplayName.Replace(" ", "") == NewSemesterName.Replace(" ", "")).Count() > 0)
                    return false;
                return true;
            }, (p) => CreateNewSemester());

            AddFromExcelCommand = new RelayCommand<object>((p) =>
            {
                return !(SelectedSemester.CourseRegisterStatus > 0);
            }, (p) => AddFromExcel());
            SaveChangesCommand = new RelayCommand<object>((p) =>
            {
                return !(SelectedSemester.CourseRegisterStatus > 0);
            }, (p) => SaveChanges());
            ConvertChangesCommand = new RelayCommand<object>((p) =>
            {
                return !(SelectedSemester.CourseRegisterStatus > 0);
            }, (p) => ConvertChanges());
        }
        public void SelectData()
        {
            CourseRegistryItems = CourseRegistryItemsAll[SelectedSemesterIndex];
            CourseRegistryItemsDisplay = CourseRegistryItems;
        }
        public void SwitchSearchButtonFunction(object p)
        {
            IsFirstSearchButtonEnabled = !IsFirstSearchButtonEnabled;
        }

        public void SearchCourseRegistryItemsFunction()
        {
            if (SearchQuery == "" || SearchQuery == null)
            {
                CourseRegistryItemsDisplay = CourseRegistryItems;
                return;
            }
            if (!IsFirstSearchButtonEnabled)
            {
                //Thiếu Code
                /*var tmp = CourseRegistryItems.Where(x => x.SubjectClassCode.ToLower().Contains(SearchQuery.ToLower())).ToList();
                CourseRegistryItemsDisplay = new ObservableCollection<CourseRegistryItem>(tmp);*/
            }
            else
            {
                var tmp = CourseRegistryItems.Where(x => vietnameseStringNormalizer.Normalize(x.Subject.DisplayName).ToLower().Contains(vietnameseStringNormalizer.Normalize(SearchQuery.ToLower()))).ToList();
                CourseRegistryItemsDisplay = new ObservableCollection<CourseItems>(tmp);
            }
        }
        public void DeleteSelectedItems()
        {
            var SelectedItems = CourseRegistryItems.Where(x => x.IsSelected == true).ToList();
            foreach (CourseItems item in SelectedItems)
            {
                item.IsSelected = false;
                CourseRegistryItems.Remove(item);
            }
            SearchCourseRegistryItemsFunction();
        }
        public void CreateNewCourse()
        {
            _creatNewCourseViewModel = new CreateNewCourseViewModel(new SubjectClass(), SelectedSemester, CourseRegistryItems);
            this.DialogItemViewModel = this._creatNewCourseViewModel;
        }

        public void CreateNewSemester()
        {
            try
            {
                Semester temp = new Semester() { Batch = SelectedBatch, CourseRegisterStatus = 0, DisplayName = NewSemesterName, Id = Guid.NewGuid() };
                Semesters.Add(temp);
                SemesterServices.Instance.SaveSemesterToDatabase(temp);
                IsDoneVisible = true;
                var courseItemsNewSemester = new ObservableCollection<CourseItems>() { };
                CourseRegistryItemsAll.Add(courseItemsNewSemester);
                SelectedSemester = Semesters.Last();
                Semesters = new ObservableCollection<Semester>(Semesters.OrderBy(y => y.DisplayName).OrderBy(x => x.Batch).ToList());
                CreateNewBatch();
            }
            catch
            {
                IsErrorVisible = true;
            }
        }

        public void CreateNewBatch()
        {
            var temp = Semesters.Select(x => x.Batch).Distinct().ToList();
            Batches = new ObservableCollection<string>(temp);
            string defaultNewBatch = "";
            foreach (string year in Batches.Last().Split('-'))
            {
                defaultNewBatch += Convert.ToString(Convert.ToInt32(year) + 1) + '-';
            }
            defaultNewBatch = defaultNewBatch.Remove(defaultNewBatch.Length - 1);
            Batches.Add(defaultNewBatch);
        }
        DataTableCollection dataSheets;
        public void AddFromExcel()
        {
            using (OpenFileDialog op = new OpenFileDialog() { Filter = "Excel|*.xls;*.xlsx;" })
            {
                if (op.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = File.Open(op.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });
                            dataSheets = result.Tables;
                        }
                    }
                    DataTable data = dataSheets[0];

                    ObservableCollection<CourseItems> excelList = CourseRegistryItems;
                    foreach (DataRow course in data.Rows)
                    {
                        var tempSubjectClass = new SubjectClass()
                        {
                            Id = Guid.NewGuid(),
                            Semester = SelectedSemester,
                            Subject = SubjectServices.Instance.FindSubjectBySubjectName(Convert.ToString(course[0])),   //Column SubjectName NVARCHAR
                            StartDate = Convert.ToDateTime(course[1]),                                                  //Column StartDate Date
                            EndDate = Convert.ToDateTime(course[2]),                                                  //Column EndDate Date
                            Period = Convert.ToString(course[3]),                                                       //Column Period NVARCHAR
                            WeekDay = Convert.ToString(course[4]),                                                      //Column WeekDay NVARCHAR
                            //Thiếu data
                            /*SubjectClassCode,
                            MaxOfRegister,
                            TrainingForm, 
                            Teachers,*/
                        };
                        var tempCourse = new CourseItems(tempSubjectClass, false);
                        excelList.Add(tempCourse);
                    }
                }
            }
            /*DataTable data = dataSheets[dataSheets.Cast<DataTable>().Select(t=>t.TableName).Last().ToString()];*/
            
        }

        public void SaveChanges()
        {
            
        }

        public void ConvertChanges()
        {

        }
    }
}
