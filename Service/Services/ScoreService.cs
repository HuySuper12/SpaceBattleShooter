using Repository.Entities;
using Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class ScoreService
    {
        private readonly ScoreRepository _scoreRepository=new();

        public List<ScoreSorted> GetScore()
        {
            return _scoreRepository.getSortedList();
        }

        public void AddScore(Score score)
        {
            _scoreRepository.addScore(score);
        }
    }
}
