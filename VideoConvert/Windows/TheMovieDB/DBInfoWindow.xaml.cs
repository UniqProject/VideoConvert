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
using System.Collections.ObjectModel;

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

        private ObservableCollection<MovieDBImageInfo> _backdropsList;

        private ObservableCollection<MovieDBPosterImage> _postersList;

        private ObservableCollection<MovieDBBannerImage> _bannerList;
        private ObservableCollection<MovieDBSeasonBannerImage> _seasonBannerList;
        private ObservableCollection<MovieDBBannerImage> _previewBannerList;

        private ObservableCollection<DBTvShowSeason> _seasonList; 

        private MovieDBCastList _castList;

        private readonly TvdbHandler _tvDbclient;
        private readonly TMDbClient _tmDbClient;

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
            _backdropsList = new ObservableCollection<MovieDBImageInfo>();

            _postersList = new ObservableCollection<MovieDBPosterImage>();

            _bannerList = new ObservableCollection<MovieDBBannerImage>();
            _seasonBannerList = new ObservableCollection<MovieDBSeasonBannerImage>();

            _previewBannerList = new ObservableCollection<MovieDBBannerImage>();

            _seasonList = new ObservableCollection<DBTvShowSeason>();

            _castList = new MovieDBCastList();
        }

        private void MovieInfo_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchString))
                MediaTitleInfo.Text = SearchString;

            DataSource.SelectedIndex = AppSettings.LastSelectedSource;
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

            Regex searchObj = new Regex(regexSearch, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
            Match matchResult = searchObj.Match(searchMedia);

            // check first if we can use the search string
            if (!matchResult.Success)
            {
                regexSearch = regexSearch.Replace(@"(?<episodename>[\w\s]*)", "").Trim().TrimEnd(new []{'-'}).Trim();
                searchObj = new Regex(regexSearch, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
                matchResult = searchObj.Match(searchMedia);
                if (!matchResult.Success) return;
            }

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
                resultShow = searchResults.Count > 0 ? searchResults.First() : null;

            if (resultShow == null || resultShow.Id == 0) return;

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
            TvShowSeason.SelectedIndex = _seasonList.ToList().FindIndex(season => season.SeasonNumber == tempInt);

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
            _bannerList.ToList().ForEach(image => _previewBannerList.Add(image));
            _seasonBannerList.ToList().ForEach(image => _previewBannerList.Add(image));

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

            MovieGenre.Text = searchResult.Genres != null
                                  ? string.Join(" / ", searchResult.Genres.ConvertAll(input => input.Name))
                                  : string.Empty;

            MovieRuntime.Text = searchResult.Runtime.ToString("g");

            if (AppSettings.MovieDBRatingSource == 0)
            {
                MovieRating.Text = searchResult.VoteAverage.ToString("g");
                MovieVotes.Text = searchResult.VoteCount.ToString("g");
            }
            else
            {
                ImdbClient imdb = new ImdbClient();
                ImdbMovieData movieData = imdb.GetMovieById(searchResult.ImdbId);
                MovieRating.Text = movieData.Rating.ToString("g");
                MovieVotes.Text = movieData.RatingCount.ToString("g");
            }

            MovieYear.Text = searchResult.ReleaseDate.Year.ToString("g");
            MovieTagline.Text = searchResult.Tagline;
            MoviePlot.Text = searchResult.Overview;

            if (movieKeywords != null && movieKeywords.Keywords != null)
                MovieKeywords.Text = string.Join(", ", movieKeywords.Keywords.ConvertAll(input => input.Name));
            else
                MovieKeywords.Text = string.Empty;

            MovieImdbId.Text = searchResult.ImdbId;

            MovieCountry.Text = searchResult.ProductionCountries != null
                               ? string.Join(" / ", searchResult.ProductionCountries.ConvertAll(input => input.Name))
                               : string.Empty;

            if (movieCasts != null && movieCasts.Crew != null)
            {
                MovieDirector.Text = string.Join(" / ",
                                                 movieCasts.Crew.Where(crew => crew.Job == "Director")
                                                           .ToList()
                                                           .ConvertAll(input => input.Name));
                MovieWriters.Text = string.Join(" / ",
                                                movieCasts.Crew.Where(
                                                    crew => crew.Job == "Writer" || crew.Job == "Screenplay")
                                                          .ToList()
                                                          .ConvertAll(input => input.Name));
            }
            else
            {
                MovieDirector.Text = string.Empty;
                MovieWriters.Text = string.Empty;
            }

            MovieStudio.Text = searchResult.ProductionCompanies != null
                                   ? string.Join(" / ", searchResult.ProductionCompanies.ConvertAll(input => input.Name))
                                   : string.Empty;

            MovieSetName.Text = searchResult.BelongsToCollection != null
                                    ? string.Join(" / ",
                                                  searchResult.BelongsToCollection.ConvertAll(input => input.Name))
                                    : string.Empty;

            if (movieTrailers != null && movieTrailers.Youtube != null && movieTrailers.Youtube.Count > 0)
                MovieTrailer.Text = "plugin://plugin.video.youtube/?action=play_video&amp;videoid=" +
                                    movieTrailers.Youtube.First().Source;
            else
                MovieTrailer.Text = string.Empty;

            Country selCountry =
                movieReleases.Countries.SingleOrDefault(country => country.Iso_3166_1.ToLowerInvariant() == selectedCertCountry);
            string certPrefix = AppSettings.MovieDBPreferredCertPrefix;

            if (selCountry == null)
            {
                selCountry =
                    movieReleases.Countries.SingleOrDefault(
                        country => country.Iso_3166_1.ToLowerInvariant() == fallbackCertCountry);
                certPrefix = AppSettings.MovieDBFallbackCertPrefix;
            }

            if (selCountry == null)
            {
                selCountry = movieReleases.Countries.First();
                certPrefix = string.Empty;
            }

            MovieMPAARating.Text = certPrefix + selCountry.Certification;

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
            MoviePosterList.ItemsSource = _postersList;
            MoviePosterList.SelectedIndex = 0;

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
            MovieBackdropList.ItemsSource = _backdropsList;
            MovieBackdropList.SelectedIndex = 0;

            foreach (Cast cast in movieCasts.Cast)
            {
                _castList.Casts.Add(new MovieDBCast
                    {
                        Name = cast.Name,
                        Role = cast.Character,
                        Thumbnail = _tmDbClient.GetImageUrl("original", cast.ProfilePath).AbsoluteUri
                    });
            }
            MovieCastListView.ItemsSource = _castList.Casts;

            ResultTabControl.SelectedIndex = 1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            float tempRating;
            int tempRuntime;
            switch (ResultTabControl.SelectedIndex)
            {
                case 1:
                    int tempVotes;
                    int tempYear;
                    float.TryParse(MovieRating.Text, NumberStyles.Float, AppSettings.CInfo, out tempRating);
                    int.TryParse(MovieRuntime.Text, NumberStyles.Integer, AppSettings.CInfo, out tempRuntime);
                    int.TryParse(MovieVotes.Text, NumberStyles.Integer, AppSettings.CInfo, out tempVotes);
                    int.TryParse(MovieYear.Text, NumberStyles.Integer, AppSettings.CInfo, out tempYear);

                    ResultMovieData = new MovieEntry
                        {
                            Casts = _castList.Casts,
                            Aired = "1969-12-31",
                            Premiered = "1969-12-31",
                            Code = "",
                            Countries =
                                MovieCountry.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            DateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Directors =
                                MovieDirector.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            EpBookmark = 0f,
                            FanartImages = _backdropsList.ToList(),
                            Genres =
                                MovieGenre.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            ImdbID = MovieImdbId.Text,
                            LastPlayed = "1969-12-31",
                            MPAARating = MovieMPAARating.Text,
                            OriginalTitle = MovieOriginalTitle.Text,
                            Outline = "",
                            PlayCount = 0,
                            Plot = MoviePlot.Text,
                            PosterImages = _postersList.ToList(),
                            Rating = tempRating,
                            Runtime = tempRuntime,
                            SetNames =
                                MovieSetName.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            SortTitle = MovieSortTitle.Text,
                            Status = "",
                            Studios =
                                MovieStudio.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            Tagline = MovieTagline.Text,
                            Title = MovieTitle.Text,
                            Top250 = 0,
                            Trailer = MovieTrailer.Text,
                            Votes = tempVotes,
                            Writers =
                                MovieWriters.Text.Split(new[] {" / "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
                            Year = tempYear,
                            SelectedBackdropImage = ((MovieDBImageInfo) MovieBackdropList.SelectedItem).UrlOriginal,
                            SelectedPosterImage = ((MovieDBPosterImage) MoviePosterList.SelectedItem).UrlOriginal
                        };
                    break;
                case 2:
                    float.TryParse(TvShowEpisodeRating.Text, NumberStyles.Float, AppSettings.CInfo, out tempRating);
                    int.TryParse(TvShowEpisodeRuntime.Text, NumberStyles.Integer, AppSettings.CInfo, out tempRuntime);
                    ResultEpisodeData = new EpisodeEntry
                        {
                            Title = TvShowEpisodeTitle.Text,
                            ImdbID = TvShowEpisodeImdbId.Text,
                            DateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Code = "",
                            LastPlayed = "1969-12-31",
                            Aired = TvShowEpisodeFirstAired.Text,
                            Rating = tempRating,
                            Runtime = tempRuntime,
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
            SearchString = MediaTitleInfo.Text;
            AppSettings.LastSelectedSource = DataSource.SelectedIndex;
            DialogResult = true;
        }

        public void Dispose()
        {
            _tvDbclient.CloseCache();
        }

        private void DataSource_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                switch (DataSource.SelectedIndex)
                {
                    case 0:
                        SearchLanguage.ItemsSource = MovieDBLanguages.LangList;
                        SearchLanguage.SelectedValuePath = "Code";
                        SearchLanguage.SelectedValue = !string.IsNullOrEmpty(AppSettings.MovieDBLastLanguage)
                                                           ? AppSettings.MovieDBLastLanguage
                                                           : "en";
                        RatingCountry.ItemsSource = MovieDBCertCountries.CountryList;
                        RatingCountry.SelectedValue = !string.IsNullOrEmpty(AppSettings.MovieDBLastRatingCountry)
                                                          ? AppSettings.MovieDBLastRatingCountry
                                                          : "us";
                        break;
                    default:
                        SearchLanguage.ItemsSource = _tvDbclient.Languages;
                        SearchLanguage.SelectedValuePath = "Abbriviation";
                        SearchLanguage.SelectedValue = !string.IsNullOrEmpty(AppSettings.TvDBPreferredLanguage)
                                                           ? AppSettings.TvDBPreferredLanguage
                                                           : "en";
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            
        }

        private void ChangeMediaTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResultTabControl.SelectedIndex != 2) return;
            MovieTitle.Text = TvShowTitle.Text + " - " + TvShowEpisodeTitle.Text;
            MovieOriginalTitle.Text = TvShowEpisodeTitle.Text;
            MovieGenre.Text = TvShowGenre.Text;
            MovieRating.Text = TvShowEpisodeRating.Text;
            MovieRuntime.Text = TvShowEpisodeRuntime.Text;
            MovieYear.Text =
                DateTime.ParseExact(TvShowEpisodeFirstAired.Text, "yyyy-MM-dd", AppSettings.CInfo)
                        .Year.ToString("g");
            MoviePlot.Text = TvShowEpisodePlot.Text;
            MovieMPAARating.Text = TvShowMpaaRating.Text;
            MovieImdbId.Text = TvShowEpisodeImdbId.Text;
            MovieDirector.Text = TvShowEpisodeDirector.Text;
            MovieWriters.Text = TvShowEpisodeWriter.Text;
            MovieStudio.Text = TvShowNetwork.Text;

            MovieBackdropList.ItemsSource = _backdropsList;

            string episodeImage = ((DBTvShowEpisode) TvShowEpisodeNumber.SelectedItem).EpisodeImageUrl;

            if (!string.IsNullOrEmpty(episodeImage))
            {
                _backdropsList.Add(new MovieDBImageInfo
                    {
                        Title = "Episode Image",
                        UrlOriginal = episodeImage,
                        UrlPreview = episodeImage
                    });
                MovieBackdropList.SelectedValue = episodeImage;
            }
            else
                MovieBackdropList.SelectedIndex = TvShowFanartList.SelectedIndex;

            MoviePosterList.ItemsSource = _postersList;
            MoviePosterList.SelectedIndex = TvShowPosterList.SelectedIndex;

            MovieCastListView.ItemsSource = _castList.Casts;

            ResultTabControl.SelectedIndex = 1;
        }

        private void AddBackdropButton_Click(object sender, RoutedEventArgs e)
        {
            ImageAddWin addWin = new ImageAddWin {Owner = this};
            if (addWin.ShowDialog() != true) return;

            MovieDBImageInfo image = new MovieDBImageInfo
                {
                    Title = "User Image",
                    UrlOriginal = addWin.ResultImage,
                    UrlPreview = addWin.ResultPreview
                };
            _backdropsList.Add(image);
        }

        private void AddPosterButton_Click(object sender, RoutedEventArgs e)
        {
            ImageAddWin addWin = new ImageAddWin { Owner = this };
            if (addWin.ShowDialog() != true) return;

            MovieDBPosterImage image = new MovieDBPosterImage
            {
                Title = "User Image",
                UrlOriginal = addWin.ResultImage,
                UrlPreview = addWin.ResultPreview
            };
            _postersList.Add(image);
        }

        private void GenerateTitleButton_Click(object sender, RoutedEventArgs e)
        {
            switch (ResultTabControl.SelectedIndex)
            {
                case 1:
                    MediaTitleInfo.Text = MovieTitle.Text;
                    break;
                case 2:
                    //Episode = ((DBTvShowEpisode) TvShowEpisodeNumber.SelectedItem).EpisodeNumber,
                            //Season = ((DBTvShowSeason) TvShowSeason.SelectedItem).SeasonNumber,
                    MediaTitleInfo.Text =
                        AppSettings.TvDBParseString.Replace("%show%", TvShowTitle.Text)
                            .Replace("%season%", ((DBTvShowSeason) TvShowSeason.SelectedItem).SeasonNumber.ToString("D2"))
                            .Replace("%episode%",
                                ((DBTvShowEpisode) TvShowEpisodeNumber.SelectedItem).EpisodeNumber.ToString("D2"))
                            .Replace("%episode_name%", TvShowEpisodeTitle.Text);
                    break;
            }
        }
    }
}
