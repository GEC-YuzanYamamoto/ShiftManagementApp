namespace ShiftFrontend.Utils
{
    // カレンダー表示に関する日付計算を行うユーティリティ
    // （UIや状態を持たない純粋ロジック）
    public static class CalendarUtil
    {
        // 指定した月のカレンダー表示情報を計算する
        // 表示対象の月（必ずその月の1日を指定する）
        // カレンダー表示開始日（日曜日）
        // 表示に必要な週数（通常 5 or 6）
        public static void CalculateMonth(
            DateTime targetMonth,
            out DateTime calendarStartDate,
            out int weeksNeeded)
        {
            // targetMonth は「月初」に丸める
            var firstDayOfMonth = new DateTime(
                targetMonth.Year,
                targetMonth.Month,
                1);

            // 当月1日が何曜日か（日=0, 月=1,...）
            int diff = (int)firstDayOfMonth.DayOfWeek;

            // カレンダー表示開始日（必ず日曜日）
            calendarStartDate = firstDayOfMonth.AddDays(-diff);

            // 当月の最終日
            var lastDayOfMonth = new DateTime(
                targetMonth.Year,
                targetMonth.Month,
                DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month));

            // カレンダー表示上の最終日（土曜日）
            var lastDayOfCalendar = lastDayOfMonth.AddDays(
                6 - (int)lastDayOfMonth.DayOfWeek);

            // 表示に必要な週数
            weeksNeeded =
                ((lastDayOfCalendar - calendarStartDate).Days + 1) / 7;
        }

        // カレンダー表示期間（DateOnly）を取得する
        public static (DateOnly Start, DateOnly End) GetDisplayRange(
            DateTime calendarStartDate,
            int weeksNeeded)
        {
            var start = DateOnly.FromDateTime(calendarStartDate);
            var end = DateOnly.FromDateTime(
                calendarStartDate.AddDays(weeksNeeded * 7 - 1));

            return (start, end);
        }
    }
}
