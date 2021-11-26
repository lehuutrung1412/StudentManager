﻿using StudentManagement.Models;
using StudentManagement.Objects;
using StudentManagement.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement.Services
{
    public class TrainingFormServices
    {
        private static TrainingFormServices s_instance;

        public static TrainingFormServices Instance => s_instance ?? (s_instance = new TrainingFormServices());

        public TrainingFormServices() { }

        public DbSet<TrainingForm> LoadTrainingFormList()
        {
            return DataProvider.Instance.Database.TrainingForms;
        }

        /// <summary>
        /// Convert TrainingFormCard To TrainingForm
        /// </summary>
        /// <param name="trainingFormCard"></param>
        /// <returns>TrainingForm</returns>
        public TrainingForm ConvertTrainingFormCardToTrainingForm(TrainingFormCard trainingFormCard)
        {
            TrainingForm trainingForm = new TrainingForm()
            {
                Id = trainingFormCard.Id,
                DisplayName = trainingFormCard.DisplayName
            };

            return trainingForm;
        }

        /// <summary>
        /// Convert TrainingForm To TrainingForm Card
        /// </summary>
        /// <param name="trainingForm"></param>
        /// <returns>TrainingFormCard</returns>
        public TrainingFormCard ConvertTrainingFormToTrainingFormCard(TrainingForm trainingForm)
        {
            TrainingFormCard trainingFormCard = new TrainingFormCard(trainingForm.Id, trainingForm.DisplayName, 5, 100);

            return trainingFormCard;
        }

        /// <summary>
        /// Find TrainingForm By TrainingForm Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>TrainingForm || null</returns>
        public TrainingForm FindTrainingFormByTrainingFormId(Guid id)
        {
            TrainingForm trainingForm = DataProvider.Instance.Database.TrainingForms.Where(trainingFormItem => trainingFormItem.Id == id).FirstOrDefault();

            return trainingForm;
        }

        /// <summary>
        /// Save TrainingForm To Database
        /// </summary>
        /// <param name="trainingForm"></param>
        public void SaveTrainingFormToDatabase(TrainingForm trainingForm)
        {
            TrainingForm savedTrainingForm = FindTrainingFormByTrainingFormId(trainingForm.Id);

            if (savedTrainingForm == null)
            {
                DataProvider.Instance.Database.TrainingForms.Add(trainingForm);
            }
            else
            {
                Reflection.CopyProperties(trainingForm, savedTrainingForm);
            }
            DataProvider.Instance.Database.SaveChanges();
        }

        /// <summary>
        /// Save TrainingForm Card To Database
        /// </summary>
        /// <param name="trainingFormCard"></param>
        public void SaveTrainingFormCardToDatabase(TrainingFormCard trainingFormCard)
        {
            TrainingForm trainingForm = ConvertTrainingFormCardToTrainingForm(trainingFormCard);

            SaveTrainingFormToDatabase(trainingForm);
        }

        /// <summary>
        /// Remove TrainingForm From Database
        /// </summary>
        /// <param name="traingingForm"></param>
        public void RemoveTrainingFormFromDatabase(TrainingForm traingingForm)
        {
            TrainingForm savedTrainingForm = FindTrainingFormByTrainingFormId(traingingForm.Id);

            DataProvider.Instance.Database.TrainingForms.Remove(savedTrainingForm);

            DataProvider.Instance.Database.SaveChanges();
        }

        /// <summary>
        /// Remove TrainingFormCard From Database
        /// </summary>
        /// <param name="traingingFormCard"></param>
        public void RemoveTrainingFormCardFromDatabase(TrainingFormCard traingingFormCard)
        {
            TrainingForm traingingForm = ConvertTrainingFormCardToTrainingForm(traingingFormCard);

            RemoveTrainingFormFromDatabase(traingingForm);
        }
    }
}