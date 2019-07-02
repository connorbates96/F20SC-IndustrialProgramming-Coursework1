using System;

public class BrowserData
{
    LinkedList<HistoryToken> history;
    List<FavouriteToken> favourites;

    public BrowserData()
    {
        history = new LinkedList<HistoryToken>();
        favourites = new List<FavouriteToken>();
    }

    struct HistoryToken
    {
        string url;
        DateTime dateTime;
        string timestamp;

        HistoryToken(string url, DateTime dateTime)
        {
            this.url = url;
            this.dateTime = dateTime;
            this.timestamp = getTimestamp(dateTime);
        }
    }

    struct FavouriteToken
    {
        string title;
        string url;
        DateTime dateTime;
        string timestamp;

        FavouriteToken(string title, string url, DateTime dateTime)
        {
            this.title = title;
            this.url = url;
            this.dateTime = dateTime;
            this.timestamp = getTimestamp(dateTime);
        }
    }
    /// <summary>
    /// Returns a DateTime value as a string in the form "yyyyMMddHHmmssfff"
    /// </summary>
    /// <param name="value">DateTime value</param>
    /// <returns>Date time as a string</returns>
    public static string getTimestamp(this DateTime value)
    {
        return value.ToString("yyyyMMddHHmmssfff");
    }

}