using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using TvdbLib;
using TvdbLib.Cache;
using TvdbLib.Data;
using TvdbLib.Data.Banner;
using VideoConvert.Core;
using VideoConvert.Core.Helpers;
using VideoConvert.Core.Helpers.IMDB;
using VideoConvert.Core.Helpers.TheMovieDB;
using log4net;

namespace VideoConvert.Windows.TheMovieDB
{
    /// <summary>
    /// Interaktionslogik für DBInfoWindow.xaml
    /// </summary>
    public partial class DBInfoWindow : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DBInfoWindow));

        public string SearchString;

        public MovieEntry ResultMovieData;
        public EpisodeEntry ResultEpisodeData;

        private List<MovieDBImageInfo> _backdropsList;

        private List<MovieDBPosterImage> _postersList;
        private List<MovieDBSeasonPosterImage> _seasonPosterList;
 
        private List<MovieDBBannerImage> _bannerList;
        private List<MovieDBSeasonBannerImage> _seasonBannerList;
        private List<MovieDBBannerImage> _previewBannerList;

        private MovieDBImageInfo _episodeImage;

        private List<DBTvShowSeason> _seasonList; 

        private MovieDBCastList _castList;

        private TvdbHandler _tvDbclient;
        private TMDbClient _tmDbClient;

        public DBInfoWindow()
        {
            InitializeComponent();
            InitLists();

            #region TvDB Client configuration

            AppSettings.InitTvDBCache();
            _tvDbclient = new TvdbHandler(new XmlCacheProvider(AppSettings.TvDBCachePath), AppSettings.TheTVDBApiKey);
            _tvDbclient.InitCache();

            #endregion


            #region MovieDB Client configuration

            _tmDbClient = new TMDbClient(AppSettings.MovieDBApiKey);
            FileInfo configXml = new FileInfo("TMDbconfig.xml");
            if (configXml.Exists && configXml.LastWriteTimeUtc >= DateTime.UtcNow.AddHours(-1))
            {
                Log.Info("TMDbClient: Using stored config");
                string xml = File.ReadAllText(configXml.FullName, Encoding.Unicode);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                _tmDbClient.SetConfig(TMDbSerializer.Deserialize<TMDbConfig>(xmlDoc));
            }
            else
            {
                Log.Info("TMDbClient: Getting new config");
                _tmDbClient.GetConfig();

                Log.Info("TMDbClient: Storing config");
                XmlDocument xmlDoc = TMDbSerializer.Serialize(_tmDbClient.Config);
                File.WriteAllText(configXml.FullName, xmlDoc.OuterXml, Encoding.Unicode);
            }

            #endregion
        }

        private void InitLists()
        {
            _backdropsList = new List<MovieDBImageInfo>();

            _postersList = new List<MovieDBPosterImage>();
            _seasonPosterList = new List<MovieDBSeasonPosterImage>();

            _bannerList = new List<MovieDBBannerImage>();
            _seasonBannerList = new List<MovieDBSeasonBannerImage>();

            _previewBannerList = new List<MovieDBBannerImage>();

            _episodeImage = new MovieDBImageInfo();

            _seasonList = new List<DBTvShowSeason>();

            _castList = new MovieDBCastList();
        }

        private void MovieInfo_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchString))
                MediaTitleInfo.Text = SearchString;

            DataSource.SelectedIndex = 0;
        }

        private void MediaInfoLoadButton_Click(object sender, RoutedEventArgs e)
        {
            string searchMedia = MediaTitleInfo.Text;
            if (string.IsNullOrEmpty(searchMedia)) return;

            string selectedLang = (string) SearchLanguage.SelectedValue;
            if (string.IsNullOrEmpty(selectedLang))
                selectedLang = "en";
            
            string selectedCertCountry = (string) RatingCountry.SelectedValue;
            if (string.IsNullOrEmpty(selectedCertCountry))
                selectedCertCountry = "us";

            string fallbackLang = AppSettings.MovieDBLastFallbackLanguage;
            string fallbackCertCountry = AppSettings.MovieDBLastFallbackRatingCountry;

            switch (DataSource.SelectedIndex)
            {
                case 0:
                    GetMovieDBInfo(searchMedia, selectedLang, fallbackLang, selectedCertCountry, fallbackCertCountry);
                    break;
                default:
                    GetTvDBInfo(searchMedia, selectedLang, fallbackLang);
                    break;
            }
        }

        private void GetTvDBInfo(string searchMedia, string selectedLang, string fallbackLang)
        {
            const string tvDBFanartPath = "http://thetvdb.com/banners/";

            InitLists();

            string regexSearch = AppSettings.TvDBParseString;
            regexSearch =
                regexSearch.Replace("%show%", @"(?<show>[\w\s]*)")
                           .Replace("%season%", @"(?<season>[\d]*)")
                           .Replace("%episode%", @"(?<episode>[\d]*)")
                           .Replace("%episode_name%", @"(?<episodename>[\w\s]*)");

            Regex searchObj = new Regex(regexSearch, RegexOptions.Multiline | RegexOptions.Singleline);
            Match matchResult = searchObj.Match(searchMedia);

            // check first if we can use the search string
            if (!matchResult.Success) return;

            TvdbLanguage dbLang = _tvDbclient.Languages.Single(language => language.Abbriviation == selectedLang) ??
                                  _tvDbclient.Languages.Single(language => language.Abbriviation == fallbackLang);

            List<TvdbSearchResult> searchResults = _tvDbclient.SearchSeries(matchResult.Groups["show"].Value, dbLang);

            if (searchResults == null) return;

            TvdbSearchResult resultShow = new TvdbSearchResult();
            if (searchResults.Count > 1)
            {
                DBMultipleSelection selectWindow = new DBMultipleSelection
                {
                    Owner = this,
                    TvdbSearchResults = searchResults
                };
                if (selectWindow.ShowDialog() == true)
                    resultShow = selectWindow.TvdbSelectionResult;
            }
            else
                resultShow = searchResults.First();

            if (resultShow.Id == 0) return;

            TvdbSeries series = _tvDbclient.GetSeries(resultShow.Id, dbLang, true, true, true, true);

            TvShowTitle.Text = series.SeriesName;
            TvShowGenre.Text = string.Join(" / ", series.Genre);
            TvShowRating.Text = series.Rating.ToString("g", AppSettings.CInfo);
            TvShowRuntime.Text = series.Runtime.ToString("g", AppSettings.CInfo);
            TvShowFirstAired.Text = series.FirstAired.ToString("yyyy-MM-dd");
            TvShowPlot.Text = series.Overview;
            TvShowMpaaRating.Text = series.ContentRating;
            TvShowImdbId.Text = series.ImdbId;
            TvShowNetwork.Text = series.Network;

            foreach (TvdbEpisode episode in series.Episodes)
            {
                DBTvShowSeason season;
                try
                {
                    season = _seasonList.First(showSeason => showSeason.SeasonNumber == episode.SeasonNumber);
                }
                catch (InvalidOperationException)
                {
                    season = new DBTvShowSeason
                    {
                        SeasonNumber = episode.SeasonNumber,
                        Title = episode.IsSpecial ? "Special" : "Season " + episode.SeasonNumber
                    };
                    _seasonList.Add(season);
                }

                season.Episodes.Add(new DBTvShowEpisode
                    {
                        Directors = episode.Directors,
                        Writers = episode.Writer,
                        SeasonNumber = episode.SeasonNumber,
                        Runtime = (int) series.Runtime,
                        Rating = episode.Rating,
                        Plot = episode.Overview,
                        IsSpecial = episode.IsSpecial,
                        ImdbId = episode.ImdbId,
                        GuestStars = episode.GuestStars,
                        FirstAired = episode.FirstAired,
                        EpisodeTitle = episode.EpisodeName,
                        EpisodeNumber = episode.EpisodeNumber,
                        DvdEpisodeNumber = episode.DvdEpisodeNumber,
                        CombinedEpisodeNumber = episode.CombinedEpisodeNumber,
                        AbsoluteEpisodeNumber = episode.AbsoluteNumber,
                        EpisodeImageUrl = tvDBFanartPath + episode.BannerPath,
                    });
            }

            TvShowSeason.ItemsSource = _seasonList;

            int tempInt;
            int.TryParse(matchResult.Groups["season"].Value, NumberStyles.Integer, AppSettings.CInfo, out tempInt);
            TvShowSeason.SelectedIndex = _seasonList.FindIndex(season => season.SeasonNumber == tempInt);

            int.TryParse(matchResult.Groups["episode"].Value, NumberStyles.Integer, AppSettings.CInfo, out tempInt);
            TvShowEpisodeNumber.SelectedIndex =
                ((List<DBTvShowEpisode>) TvShowEpisodeNumber.Items.SourceCollection).FindIndex(
                    episode => episode.EpisodeNumber == tempInt);

            int imageCounter = 1;
            foreach (TvdbSeriesBanner banner in series.SeriesBanners)
            {
                _bannerList.Add(new MovieDBBannerImage
                    {
                        UrlOriginal = tvDBFanartPath + banner.BannerPath,
                        UrlPreview =
                            tvDBFanartPath + (!string.IsNullOrEmpty(banner.ThumbPath)
                                                  ? banner.ThumbPath
                                                  : "_cache/" + banner.BannerPath),
                        Title = "Online image " + imageCounter
                    });
                imageCounter++;
            }
            foreach (TvdbSeasonBanner banner in series.SeasonBanners)
            {
                _seasonBannerList.Add(new MovieDBSeasonBannerImage
                    {
                        UrlOriginal = tvDBFanartPath + banner.BannerPath,
                        UrlPreview =
                            tvDBFanartPath + (!string.IsNullOrEmpty(banner.ThumbPath)
                                                  ? banner.ThumbPath
                                                  : "_cache/" + banner.BannerPath),
                        Title = "Online image " + imageCounter,
                        Season = banner.Season
                    });
                imageCounter++;
            }
            _previewBannerList.AddRange(_bannerList);
            _previewBannerList.AddRange(_seasonBannerList);

            imageCounter = 1;
            foreach (TvdbFanartBanner banner in series.FanartBanners)
            {
                _backdropsList.Add(new MovieDBImageInfo
                    {
                        Title = "Online image " + imageCounter,
                        UrlOriginal = tvDBFanartPath + banner.BannerPath,
                        UrlPreview =
                            tvDBFanartPath + (!string.IsNullOrEmpty(banner.ThumbPath)
                                                  ? banner.ThumbPath
                                                  : "_cache/" + banner.BannerPath)
                    });
                imageCounter++;
            }

            imageCounter = 1;
            foreach (TvdbPosterBanner banner in series.PosterBanners)
            {
                _postersList.Add(new MovieDBPosterImage
                    {
                        Title = "Online image " + imageCounter,
                        UrlOriginal = tvDBFanartPath + banner.BannerPath,
                        UrlPreview =
                            tvDBFanartPath + (!string.IsNullOrEmpty(banner.ThumbPath)
                                                  ? banner.ThumbPath
                                                  : "_cache/" + banner.BannerPath)
                    });
                imageCounter++;
            }
            TvShowBannerList.ItemsSource = _previewBannerList;
            TvShowBannerList.SelectedValue = tvDBFanartPath + "_cache/" + series.BannerPath;

            TvShowFanartList.ItemsSource = _backdropsList;
            TvShowFanartList.SelectedValue = tvDBFanartPath + "_cache/" + series.FanartPath;

            TvShowPosterList.ItemsSource = _postersList;
            TvShowPosterList.SelectedValue = tvDBFanartPath + "_cache/" + series.PosterPath;

            foreach (TvdbActor actor in series.TvdbActors)
            {
                _castList.Casts.Add(new MovieDBCast
                    {
                        Name = actor.Name,
                        Role = actor.Role,
                        Thumbnail =
                            actor.ActorImage != null && !string.IsNullOrEmpty(actor.ActorImage.BannerPath)
                                ? tvDBFanartPath + actor.ActorImage.BannerPath
                                : string.Empty
                    });
            }
            TvShowCastList.ItemsSource = _castList.Casts;

            ResultTabControl.SelectedIndex = 2;
        }

        private void GetMovieDBInfo(string searchMedia, string selectedLang, string fallbackLang, string selectedCertCountry,
                                    string fallbackCertCountry)
        {
            InitLists();

            SearchContainer<SearchMovie> movieList = _tmDbClient.SearchMovie(searchMedia, selectedLang);
            if (movieList == null || movieList.TotalResults <= 0)
                movieList = _tmDbClient.SearchMovie(searchMedia, fallbackLang);

            if (movieList == null || movieList.TotalResults <= 0) return;

            SearchMovie resultMovie = new SearchMovie();
            if (movieList.TotalResults > 1)
            {
                DBMultipleSelection selectWindow = new DBMultipleSelection
                    {
                        Owner = this,
                        MovieDBSearchResults = movieList
                    };
                if (selectWindow.ShowDialog() == true)
                    resultMovie = selectWindow.MovieDBSelectionResult;
            }
            else
                resultMovie = movieList.Results.First();

            if (resultMovie.Id == 0) return;

            Movie searchResult = _tmDbClient.GetMovie(resultMovie.Id, selectedLang) ??
                                 _tmDbClient.GetMovie(resultMovie.Id, fallbackLang);

            if (searchResult == null) return;

            ImagesWithId imageList = _tmDbClient.GetMovieImages(resultMovie.Id);
            Casts movieCasts = _tmDbClient.GetMovieCasts(resultMovie.Id);
            KeywordsContainer movieKeywords = _tmDbClient.GetMovieKeywords(resultMovie.Id);
            Trailers movieTrailers = _tmDbClient.GetMovieTrailers(resultMovie.Id);
            Releases movieReleases = _tmDbClient.GetMovieReleases(resultMovie.Id);

            MovieTitle.Text = searchResult.Title;
            MovieOriginalTitle.Text = searchResult.OriginalTitle;

            Genre.Text = searchResult.Genres != null
                             ? string.Join(" / ", searchResult.Genres.ConvertAll(input => input.Name))
                             : string.Empty;

            Runtime.Text = searchResult.Runtime.ToString("g");

            if (AppSettings.MovieDBRatingSource == 0)
            {
                Rating.Text = searchResult.VoteAverage.ToString("g");
                Votes.Text = searchResult.VoteCount.ToString("g");
            }
            else
            {
                ImdbClient imdb = new ImdbClient();
                ImdbMovieData movieData = imdb.GetMovieById(searchResult.ImdbId);
                Rating.Text = movieData.Rating.ToString("g");
                Votes.Text = movieData.RatingCount.ToString("g");
            }

            Year.Text = searchResult.ReleaseDate.Year.ToString("g");
            Tagline.Text = searchResult.Tagline;
            Plot.Text = searchResult.Overview;

            if (movieKeywords != null && movieKeywords.Keywords != null)
                Keywords.Text = string.Join(", ", movieKeywords.Keywords.ConvertAll(input => input.Name));
            else
                Keywords.Text = string.Empty;

            ImdbId.Text = searchResult.ImdbId;

            Country.Text = searchResult.ProductionCountries != null
                               ? string.Join(" / ", searchResult.ProductionCountries.ConvertAll(input => input.Name))
                               : string.Empty;

            if (movieCasts != null && movieCasts.Crew != null)
            {
                Director.Text = string.Join(" / ",
                                            movieCasts.Crew.Where(crew => crew.Job == "Director")
                                                      .ToList()
                                                      .ConvertAll(input => input.Name));
                Writers.Text = string.Join(" / ",
                                           movieCasts.Crew.Where(crew => crew.Job == "Writer" || crew.Job == "Screenplay")
                                                     .ToList()
                                                     .ConvertAll(input => input.Name));
            }
            else
            {
                Director.Text = string.Empty;
                Writers.Text = string.Empty;
            }

            Studio.Text = searchResult.ProductionCompanies != null
                              ? string.Join(" / ", searchResult.ProductionCompanies.ConvertAll(input => input.Name))
                              : string.Empty;

            SetName.Text = searchResult.BelongsToCollection != null
                               ? string.Join(" / ", searchResult.BelongsToCollection.ConvertAll(input => input.Name))
                               : string.Empty;

            if (movieTrailers != null && movieTrailers.Youtube != null && movieTrailers.Youtube.Count > 0)
                Trailer.Text = "plugin://plugin.video.youtube/?action=play_video&amp;videoid=" +
                               movieTrailers.Youtube.First().Source;
            else
                Trailer.Text = string.Empty;

            Country selCountry =
                movieReleases.Countries.Single(country => country.Iso_3166_1.ToLowerInvariant() == selectedCertCountry);
            string certPrefix = AppSettings.MovieDBPreferredCertPrefix;

            if (selCountry == null)
            {
                selCountry =
                    movieReleases.Countries.Single(
                        country => country.Iso_3166_1.ToLowerInvariant() == fallbackCertCountry);
                certPrefix = AppSettings.MovieDBFallbackCertPrefix;
            }

            MPAARating.Text = certPrefix + selCountry.Certification;

            // loading image sizes
            string posterOriginal = _tmDbClient.Config.Images.PosterSizes.Last();

            string posterPreview = _tmDbClient.Config.Images.PosterSizes.Count >= 2
                                       ? _tmDbClient.Config.Images.PosterSizes[_tmDbClient.Config.Images.PosterSizes.Count - 2]
                                       : _tmDbClient.Config.Images.PosterSizes.Last();

            string backdropOriginal = _tmDbClient.Config.Images.BackdropSizes.Last();

            string backdropPreview = _tmDbClient.Config.Images.BackdropSizes.Count >= 3
                                         ? _tmDbClient.Config.Images.BackdropSizes[
                                             _tmDbClient.Config.Images.BackdropSizes.Count - 3]
                                         : _tmDbClient.Config.Images.BackdropSizes.Last();

            // remove duplicate entries
            imageList.Backdrops.RemoveAt(imageList.Backdrops.FindIndex(data => data.FilePath == searchResult.BackdropPath));
            imageList.Posters.RemoveAt(imageList.Posters.FindIndex(data => data.FilePath == searchResult.PosterPath));


            // create image lists
            _postersList.Add(new MovieDBPosterImage
                {
                    Title = "Default",
                    UrlOriginal = _tmDbClient.GetImageUrl(posterOriginal, searchResult.PosterPath).AbsoluteUri,
                    UrlPreview = _tmDbClient.GetImageUrl(posterPreview, searchResult.PosterPath).AbsoluteUri
                });
            _backdropsList.Add(new MovieDBImageInfo
                {
                    Title = "Default",
                    UrlOriginal = _tmDbClient.GetImageUrl(backdropOriginal, searchResult.BackdropPath).AbsoluteUri,
                    UrlPreview = _tmDbClient.GetImageUrl(backdropPreview, searchResult.BackdropPath).AbsoluteUri
                });

            int cnt = 1;
            foreach (ImageData poster in imageList.Posters)
            {
                _postersList.Add(new MovieDBPosterImage
                    {
                        Title = "Online image " + cnt,
                        UrlOriginal = _tmDbClient.GetImageUrl(posterOriginal, poster.FilePath).AbsoluteUri,
                        UrlPreview = _tmDbClient.GetImageUrl(posterPreview, poster.FilePath).AbsoluteUri
                    });
                cnt++;
            }
            PosterList.ItemsSource = _postersList;
            PosterList.SelectedIndex = 0;

            cnt = 1;
            foreach (ImageData backdrop in imageList.Backdrops)
            {
                _backdropsList.Add(new MovieDBImageInfo
                    {
                        Title = "Online image " + cnt,
                        UrlOriginal = _tmDbClient.GetImageUrl(backdropOriginal, backdrop.FilePath).AbsoluteUri,
                        UrlPreview = _tmDbClient.GetImageUrl(backdropPreview, backdrop.FilePath).AbsoluteUri
                    });
                cnt++;
            }
            BackdropList.ItemsSource = _backdropsList;
            BackdropList.SelectedIndex = 0;

            foreach (Cast cast in movieCasts.Cast)
            {
                _castList.Casts.Add(new MovieDBCast
                    {
                        Name = cast.Name,
                        Role = cast.Character,
                        Thumbnail = _tmDbClient.GetImageUrl("original", cast.ProfilePath).AbsoluteUri
                    });
            }
            CastListView.ItemsSource = _castList.Casts;

            ResultTabControl.SelectedIndex = 1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            switch (ResultTabControl.SelectedIndex)
            {
                case 1:
                    ResultMovieData = new MovieEntry
                        {
                            Casts = _castList.Casts,
                            Aired = "1969-12-31",
                            Premiered = "1969-12-31",
                            Code = "",
                            Countries = Country.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            DateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Directors = Director.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            EpBookmark = 0f,
                            FanartImages = _backdropsList,
                            Genres = Genre.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            ImdbID = ImdbId.Text,
                            LastPlayed = "1969-12-31",
                            MPAARating = MPAARating.Text,
                            OriginalTitle = MovieOriginalTitle.Text,
                            Outline = "",
                            PlayCount = 0,
                            Plot = Plot.Text,
                            PosterImages = _postersList,
                            Rating = float.Parse(Rating.Text, AppSettings.CInfo),
                            Runtime = int.Parse(Runtime.Text, AppSettings.CInfo),
                            SetNames = SetName.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            SortTitle = SortTitle.Text,
                            Status = "",
                            Studios = Studio.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            Tagline = Tagline.Text,
                            Title = MovieTitle.Text,
                            Top250 = 0,
                            Trailer = Trailer.Text,
                            Votes = int.Parse(Votes.Text, AppSettings.CInfo),
                            Writers = Writers.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            Year = int.Parse(Year.Text, AppSettings.CInfo),
                            SelectedBackdropImage = ((MovieDBImageInfo) BackdropList.SelectedItem).UrlOriginal,
                            SelectedPosterImage = ((MovieDBPosterImage) PosterList.SelectedItem).UrlOriginal
                        };
                    break;
                case 2:
                    ResultEpisodeData = new EpisodeEntry
                        {
                            Title = TvShowEpisodeTitle.Text,
                            ImdbID = TvShowEpisodeImdbId.Text,
                            DateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Code = "",
                            LastPlayed = "1969-12-31",
                            Aired = TvShowEpisodeFirstAired.Text,
                            Rating = float.Parse(TvShowEpisodeRating.Text, AppSettings.CInfo),
                            Runtime = int.Parse(TvShowEpisodeRuntime.Text, AppSettings.CInfo),
                            Directors =
                                TvShowEpisodeDirector.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries)
                                                     .ToList(),
                            Writers =
                                TvShowEpisodeWriter.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries)
                                                   .ToList(),
                            Plot = TvShowEpisodePlot.Text,
                            Studios = TvShowNetwork.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries)
                                                   .ToList(),
                            DisplayEpisode = ((DBTvShowEpisode) TvShowEpisodeNumber.SelectedItem).EpisodeNumber,
                            DisplaySeason = ((DBTvShowSeason) TvShowSeason.SelectedItem).SeasonNumber,
                            Episode = ((DBTvShowEpisode) TvShowEpisodeNumber.SelectedItem).EpisodeNumber,
                            Season = ((DBTvShowSeason) TvShowSeason.SelectedItem).SeasonNumber,
                            MPAARating = TvShowMpaaRating.Text,
                            ShowTitle = TvShowTitle.Text,
                            PosterImage =
                                new MovieDBImageInfo
                                    {
                                        UrlOriginal = ((DBTvShowEpisode) TvShowEpisodeNumber.SelectedItem).EpisodeImageUrl
                                    },
                            SelectedPosterImage = ((DBTvShowEpisode) TvShowEpisodeNumber.SelectedItem).EpisodeImageUrl,
                            Casts = _castList.Casts,
                            Premiered = TvShowFirstAired.Text
                        };

                    List<string> gStars = ((DBTvShowEpisode) TvShowEpisodeNumber.SelectedItem).GuestStars;
                    foreach (string gStar in gStars)
                    {
                        ResultEpisodeData.Casts.Add(new MovieDBCast{Name = gStar});
                    }

                    break;
            }
            DialogResult = true;
        }

        public void Dispose()
        {
            _tvDbclient.CloseCache();
        }

        private void DataSource_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataSource.SelectedIndex == 0)
            {
                SearchLanguage.ItemsSource = MovieDBLanguages.LangList;
                SearchLanguage.SelectedValuePath = "Code";
                SearchLanguage.SelectedValue = !string.IsNullOrEmpty(AppSettings.MovieDBLastLanguage)
                                                   ? AppSettings.MovieDBLastLanguage
                                                   : "en";
                
                RatingCountry.ItemsSource = MovieDBCertCountries.CountryList;
                RatingCountry.SelectedValue = !string.IsNullOrEmpty(AppSettings.MovieDBLastRatingCountry)
                                                  ? AppSettings.MovieDBLastRatingCountry
                                                  : "us";
            }
            else
            {
                SearchLanguage.ItemsSource = _tvDbclient.Languages;
                SearchLanguage.SelectedValuePath = "Abbriviation";
                SearchLanguage.SelectedValue = !string.IsNullOrEmpty(AppSettings.TvDBPreferredLanguage)
                                                   ? AppSettings.TvDBPreferredLanguage
                                                   : "en";
            }
        }
    }
}
