﻿using Microsoft.EntityFrameworkCore;
using RMuseum.DbContext;
using RMuseum.Models.Ganjoor;
using RSecurityBackend.Models.Generic;
using RSecurityBackend.Services;
using RSecurityBackend.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RMuseum.Services.Implementation
{
    /// <summary>
    /// numbering service
    /// </summary>
    public class GanjoorNumberingService
    {
        /// <summary>
        /// add numbering
        /// </summary>
        /// <param name="numbering"></param>
        /// <returns></returns>
        public async Task<RServiceResult<GanjoorNumbering>> AddNumberingAsync(GanjoorNumbering numbering)
        {
            numbering.Name = numbering.Name == null ? numbering.Name : numbering.Name.Trim();
            if (string.IsNullOrEmpty(numbering.Name))
                return new RServiceResult<GanjoorNumbering>(null, "ورود نام طرح شماره‌گذاری اجباری است.");
            var existing = await _context.GanjoorNumberings.Where(l => l.Name == numbering.Name).FirstOrDefaultAsync();
            if (existing != null)
                return new RServiceResult<GanjoorNumbering>(null, "اطلاعات تکراری است.");
            try
            {
                _context.GanjoorNumberings.Add(numbering);
                await _context.SaveChangesAsync();
                Count(numbering.Id); //start counting
                return new RServiceResult<GanjoorNumbering>(numbering);
            }
            catch (Exception exp)
            {
                return new RServiceResult<GanjoorNumbering>(null, exp.ToString());
            }
        }

        /// <summary>
        /// update an existing numbering (only name)
        /// </summary>
        /// <param name="updated"></param>
        /// <returns></returns>
        public async Task<RServiceResult<bool>> UpdateNumberingAsync(GanjoorNumbering updated)
        {
            try
            {
                var numbering = await _context.GanjoorNumberings.Where(l => l.Id == updated.Id).SingleOrDefaultAsync();
                if (numbering == null)
                    return new RServiceResult<bool>(false, "اطلاعات طرح شماره گذاری یافت نشد.");

                numbering.Name = updated.Name;
                _context.Update(numbering);

                await _context.SaveChangesAsync();
                return new RServiceResult<bool>(true);
            }
            catch (Exception exp)
            {
                return new RServiceResult<bool>(false, exp.ToString());
            }
        }

        /// <summary>
        /// delete numbering
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RServiceResult<bool>> DeleteNumberingAsync(int id)
        {
            try
            {
                var numbering = await _context.GanjoorNumberings.Where(l => l.Id == id).SingleOrDefaultAsync();
                if (numbering == null)
                    return new RServiceResult<bool>(false, "اطلاعات طرح شماره گذاری یافت نشد.");

                _context.Remove(numbering);

                await _context.SaveChangesAsync();
                return new RServiceResult<bool>(true);
            }
            catch (Exception exp)
            {
                return new RServiceResult<bool>(false, exp.ToString());
            }
        }

        /// <summary>
        /// get numbering by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RServiceResult<GanjoorNumbering>> GetNumberingAsync(int id)
        {
            try
            {
                return new RServiceResult<GanjoorNumbering>(await _context.GanjoorNumberings.Where(l => l.Id == id).SingleOrDefaultAsync());
            }
            catch (Exception exp)
            {
                return new RServiceResult<GanjoorNumbering>(null, exp.ToString());
            }
        }

        /// <summary>
        /// get all numberings
        /// </summary>
        /// <returns></returns>
        public async Task<RServiceResult<GanjoorNumbering[]>> GetNumberingsAsync()
        {
            try
            {
                return new RServiceResult<GanjoorNumbering[]>(await _context.GanjoorNumberings.OrderBy(l => l.Id).ToArrayAsync());
            }
            catch (Exception exp)
            {
                return new RServiceResult<GanjoorNumbering[]>(null, exp.ToString());
            }
        }

        /// <summary>
        /// get numberings for a cat
        /// </summary>
        /// <returns></returns>
        public async Task<RServiceResult<GanjoorNumbering[]>> GetNumberingsForCatAsync(int catId)
        {
            try
            {
                return new RServiceResult<GanjoorNumbering[]>(await _context.GanjoorNumberings.Where(n => n.StartCatId == catId).OrderBy(l => l.Id).ToArrayAsync());
            }
            catch (Exception exp)
            {
                return new RServiceResult<GanjoorNumbering[]>(null, exp.ToString());
            }
        }

        /// <summary>
        /// get numberings for direct subcats of parent cat
        /// </summary>
        /// <returns></returns>
        public async Task<RServiceResult<GanjoorNumbering[]>> GetNumberingsForDirectSubCatsAsync(int parentCatId)
        {
            try
            {
                var subcatIds = await _context.GanjoorCategories.AsNoTracking().Where(c => c.ParentId == parentCatId).Select(c => c.Id).ToListAsync();
                return new RServiceResult<GanjoorNumbering[]>(await _context.GanjoorNumberings.Where(n => subcatIds.Contains(n.StartCatId)).OrderBy(l => l.Id).ToArrayAsync());
            }
            catch (Exception exp)
            {
                return new RServiceResult<GanjoorNumbering[]>(null, exp.ToString());
            }
        }

        private async Task<List<GanjoorCat>> _FindAllSubCategories(RMuseumDbContext context, int catId)
        {
            List<GanjoorCat> cats = new List<GanjoorCat>();

            foreach (var cat in await context.GanjoorCategories.AsNoTracking().Where(c => c.ParentId == catId).OrderBy(c => c.Id).ToListAsync())
            {
                cats.Add(cat);

                var subCats = await _FindAllSubCategories(context, cat.Id);

                if(subCats.Count > 0)
                {
                    cats.AddRange(subCats);
                }
            }
            return cats;
        }

        private async Task<List<GanjoorCat>> _FindCategoryRanges(RMuseumDbContext context, GanjoorCat startCat, int endCatId)
        {
            var parents = await context.GanjoorCategories.AsNoTracking().Where(c => c.ParentId == startCat.ParentId && c.Id >= startCat.Id && c.Id <= endCatId).OrderBy(c => c.Id).ToListAsync();
            List<GanjoorCat> cats = new List<GanjoorCat>();
            foreach(GanjoorCat parentCat in parents)
            {
                cats.Add(parentCat);
                var subCats = await _FindAllSubCategories(context, parentCat.Id);
                if(subCats.Count > 0)
                {
                    cats.AddRange(subCats);
                }
            }
            return cats;
        }


        private async Task<List<GanjoorCat>> _FindTargetCategories(RMuseumDbContext context, GanjoorCat startCat, int? endCatId)
        {
            if(endCatId == null || startCat.Id == endCatId)
            {
                List<GanjoorCat> cats = new List<GanjoorCat>();
                cats.Add(startCat);
                cats.AddRange(await _FindAllSubCategories(context, startCat.Id));
                return cats;
            }
            else
            {
                return await _FindCategoryRanges(context, startCat, (int)endCatId);
            }
        }

        /// <summary>
        /// start counting
        /// </summary>
        /// <returns></returns>
        public RServiceResult<bool> Count(int numberingId)
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem
            (
            async token =>
            {
                using (RMuseumDbContext context = new RMuseumDbContext(new DbContextOptions<RMuseumDbContext>())) //this is long running job, so _context might be already been freed/collected by GC
                {
                    LongRunningJobProgressServiceEF jobProgressServiceEF = new LongRunningJobProgressServiceEF(context);
                    var job = (await jobProgressServiceEF.NewJob("NumberingService::Count", "Query Cats")).Result;

                    try
                    {
                        var numbering = await context.GanjoorNumberings.Include(n => n.StartCat).Where(n => n.Id == numberingId).SingleOrDefaultAsync();
                        var cats = await _FindTargetCategories(context, numbering.StartCat, numbering.EndCatId);

                        await jobProgressServiceEF.UpdateJob(job.Id, 0, "Deleting Old Data");

                        var oldNumbers = await context.GanjoorVerseNumbers.Where(n => n.NumberingId == numberingId).ToListAsync();
                        context.GanjoorVerseNumbers.RemoveRange(oldNumbers);
                        await context.SaveChangesAsync();

                        await jobProgressServiceEF.UpdateJob(job.Id, 0, "Counting");

                        int number = 0;
                        int totalVerseCount = 0;

                        foreach (var cat in cats)
                        {
                            var poems = await context.GanjoorPoems.AsNoTracking().Where(p => p.CatId == cat.Id).OrderBy(p => p.Id).ToListAsync();

                            await jobProgressServiceEF.UpdateJob(job.Id, 0, $"Counting:: {cat.Title}");

                            foreach(var poem in poems)
                            {
                                totalVerseCount += await context.GanjoorVerses.Where(v => v.PoemId == poem.Id).CountAsync();
                                var verses = await context.GanjoorVerses.AsNoTracking()
                                                .Where(v => v.PoemId == poem.Id && (v.VersePosition == VersePosition.Right || v.VersePosition == VersePosition.CenteredVerse1))
                                                .OrderBy(v => v.VOrder)
                                                .ToListAsync();
                                for (int coupletIndex = 0; coupletIndex < verses.Count; coupletIndex++)
                                {
                                    number++;
                                    GanjoorVerseNumber verseNumber = new GanjoorVerseNumber()
                                    {
                                        NumberingId = numberingId,
                                        PoemId = poem.Id,
                                        CoupletIndex = coupletIndex,
                                        Number = number
                                    };
                                    context.GanjoorVerseNumbers.Add(verseNumber);
                                }
                            }
                        }

                        numbering.TotalCouplets = number;
                        numbering.TotalVerses = totalVerseCount;
                        numbering.LastCountingDate = DateTime.Now;
                        context.GanjoorNumberings.Update(numbering);

                        await context.SaveChangesAsync();

                        await jobProgressServiceEF.UpdateJob(job.Id, 100, "", true);
                    }
                    catch (Exception exp)
                    {
                        await jobProgressServiceEF.UpdateJob(job.Id, 100, "", false, exp.ToString());
                    }

                }
            }
            );

            return new RServiceResult<bool>(true);
        }

        /// <summary>
        /// Database Context
        /// </summary>
        protected readonly RMuseumDbContext _context;

        /// <summary>
        /// Background Task Queue Instance
        /// </summary>
        protected readonly IBackgroundTaskQueue _backgroundTaskQueue;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="backgroundTaskQueue"></param>
        public GanjoorNumberingService(RMuseumDbContext context, IBackgroundTaskQueue backgroundTaskQueue)
        {
            _context = context;
            _backgroundTaskQueue = backgroundTaskQueue;
        }
    }
}
