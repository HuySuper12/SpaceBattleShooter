using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class ScoreRepository
    {
        ScoreBoardDbContext _context;

        public List<Score> getList() {
            _context = new();

            return _context.Scores.ToList();
        }

        public List<ScoreSorted> getSortedList()
        {
            _context = new();

            return _context.ScoreSorteds.OrderByDescending(s => s.Score).ToList();
        }

        public void addScore(Score score)
        {
            _context = new();

            _context.Scores.Add(score);
            _context.SaveChanges();
        }
    }
}
