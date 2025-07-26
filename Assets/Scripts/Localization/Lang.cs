using System.Collections.Generic;
using UnityEngine;
using YG;

public class Lang : MonoBehaviour
{
    private static Dictionary<string, string> ru = new Dictionary<string, string>
    {
        { "play", "Играть" },
        { "skins", "Гардероб" },
        { "howtoplay", "Как играть" },
        { "auth", "Авторизоваться" },
        { "review", "Оценить игру" },
        { "selectbiome", "Выбери биом" },
        { "summer", "Лето" },
        { "winter", "Зима" },
        { "back", "Выйти" },
        { "selectlevel", "Выбери уровень" },
        { "whyauth1", "Преимущества авторизации" },
        { "whyauth2", "Авторизация сохранит ваш прогресс в облаке, чтобы вы могли играть с любого устройства. Вы также будете получать уведомления о новинках и акциях, а купленные предметы останутся доступными, даже если вы смените браузер.\r\n\r\nМожно играть и без входа, но тогда эти возможности не будут доступны." },
        { "howtoplaytext", "Ваша задача - Собрать звёзды на уровне и дойти до финиша (желтый блок).\r\n\r\nW - Двигаться вперёд\r\nS - Двигаться назад\r\nA - Двигаться влево\r\nD - Двигаться вправо\r\nEscape - Меню паузы\r\nЗажатие левой кнопки мыши - Вращать камеру\r\n\r\nПо желанию вы можете двигаться стрелками на клавиатуре." },
        { "mobile_howtoplaytext", "Ваша задача - Собрать звёзды на уровне и дойти до финиша (желтый блок).\r\n\r\nДвижение осуществляется с помощью стрелок на экране.\r\nКасание и движение пальцем - Вращать камеру" },
        { "wintemodaltitle", "Доступ к зимним уровням" },
        { "wintemodaltext", "Чтобы играть на этих снежных уровнях, приобретите полный доступ к зимнему биому." },
        { "equip", "Надеть" },
        { "equiped", "Надето" },
        { "steps", "Количество ходов" },
        { "level", "Уровень" },
        { "pause", "Пауза" },
        { "retry", "Повторить" },
        { "continue", "Продолжить" },
        { "skiplevel", "Пропустить уровень" },
        { "gameover", "Уровень не пройден!" },
        { "finish", "Уровень пройден!" },
        { "nextlevel", "Следующий уровень" },
    };
    private static Dictionary<string, string> en = new Dictionary<string, string>
{
    { "play", "Play" },
    { "skins", "Wardrobe" },
    { "howtoplay", "How to Play" },
    { "auth", "Sign In" },
    { "review", "Rate the Game" },
    { "selectbiome", "Select Biome" },
    { "summer", "Summer" },
    { "winter", "Winter" },
    { "back", "Exit" },
    { "selectlevel", "Select Level" },
    { "whyauth1", "Benefits of Signing In" },
    { "whyauth2", "Signing in will save your progress to the cloud, allowing you to play from any device. You'll also receive notifications about updates and promotions, and purchased items will remain available even if you switch browsers.\r\n\r\nYou can play without signing in, but these features won't be available." },
    { "howtoplaytext", "Your goal: Collect stars on the level and reach the finish (yellow block).\r\n\r\nW - Move forward\r\nS - Move backward\r\nA - Move left\r\nD - Move right\r\nEscape - Pause menu\r\nHold Left Mouse Button - Rotate camera\r\n\r\nOptionally, you can use arrow keys for movement." },
    { "mobile_howtoplaytext", "Your goal: Collect stars on the level and reach the finish (yellow block).\r\n\r\nMovement is controlled by on-screen arrows.\r\nTouch and drag - Rotate camera" },
    { "wintemodaltitle", "Access to Winter Levels" },
    { "wintemodaltext", "To play these snow-covered levels, purchase full access to the winter biome." },
    { "equip", "Equip" },
    { "equiped", "Equipped" },
    { "steps", "Move Count" },
    { "level", "Level" },
    { "pause", "Pause" },
    { "retry", "Retry" },
    { "continue", "Continue" },
    { "skiplevel", "Skip Level" },
    { "gameover", "Level Failed!" },
    { "finish", "Level Complete!" },
    { "nextlevel", "Next Level" },
};
    private static Dictionary<string, string> tr = new Dictionary<string, string>
{
    { "play", "Oyna" },
    { "skins", "Kıyafetler" },
    { "howtoplay", "Nasıl Oynanır" },
    { "auth", "Giriş Yap" },
    { "review", "Oyunu Değerlendir" },
    { "selectbiome", "Biom Seç" },
    { "summer", "Yaz" },
    { "winter", "Kış" },
    { "back", "Çık" },
    { "selectlevel", "Seviye Seç" },
    { "whyauth1", "Giriş Yapmanın Avantajları" },
    { "whyauth2", "Giriş yaparak ilerlemeniz bulutta kaydedilir ve her cihazdan oynayabilirsiniz. Ayrıca güncellemeler ve promosyonlar hakkında bildirim alacaksınız, satın aldığınız eşyalar tarayıcı değiştirseniz bile kalıcı olacaktır.\r\n\r\nGiriş yapmadan da oynayabilirsiniz, ancak bu özellikler devre dışı kalır." },
    { "howtoplaytext", "Göreviniz: Seviyedeki yıldızları toplayıp bitiş noktasına (sarı blok) ulaşmak.\r\n\r\nW - İleri git\r\nS - Geri git\r\nA - Sola git\r\nD - Sağa git\r\nEscape - Duraklatma menüsü\r\nSol fare tuşunu basılı tut - Kamerayı döndür\r\n\r\nİsteğe bağlı olarak ok tuşlarını da kullanabilirsiniz." },
    { "mobile_howtoplaytext", "Göreviniz: Seviyedeki yıldızları toplayıp bitiş noktasına (sarı blok) ulaşmak.\r\n\r\nHareket, ekrandaki oklarla kontrol edilir.\r\nDokunup sürükle - Kamerayı döndür" },
    { "wintemodaltitle", "Kış Seviyelerine Erişim" },
    { "wintemodaltext", "Bu karlı seviyeleri oynamak için kış biomuna tam erişim satın alın." },
    { "equip", "Giy" },
    { "equiped", "Giyildi" },
    { "steps", "Hamle Sayısı" },
    { "level", "Seviye" },
    { "pause", "Duraklat" },
    { "retry", "Yeniden Dene" },
    { "continue", "Devam Et" },
    { "skiplevel", "Seviyeyi Atla" },
    { "gameover", "Seviye Başarısız!" },
    { "finish", "Seviye Tamamlandı!" },
    { "nextlevel", "Sonraki Seviye" },
};
    public static string Get(string key)
    {
        string lang = YG2.lang;
        if (lang == "ru")
        {
            return ru[key];
        }
        if (lang == "en")
        {
            return en[key];
        }
        if (lang == "tr")
        {
            return tr[key];
        }
        return "";
    }
}
