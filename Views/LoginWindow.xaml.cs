using System;
using System.Windows;
using System.Linq;
using Drum_Machine.Data;
using Drum_Machine.Data.Entities;
using Drum_Machine.Data.Repositories;
using Drum_Machine.Core;

namespace Drum_Machine.Views
{
    public partial class LoginWindow : Window
    {
        private readonly UserRepository _userRepository;
        private bool _isLoginMode = true;

        public LoginWindow()
        {
            InitializeComponent();

            var context = new AppDbContext();
            _userRepository = new UserRepository(context);
        }

        private void btnMain_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }

            if (_isLoginMode)
            {
                HandleLogin(username, password);
            }
            else
            {
                HandleRegistration(username, password);
            }
        }

        private void HandleLogin(string username, string password)
        {
            var user = _userRepository.Login(username, password);

            if (user != null)
            {
                AppSession.CurrentUser = user;

                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неправильний логін або пароль.");
            }
        }

        private void HandleRegistration(string username, string password)
        {
            if (password.Length < 4)
            {
                MessageBox.Show("Пароль має бути не менше 4 символів.");
                return;
            }

            try
            {
                var newUser = new User
                {
                    Username = username,
                    Password = password
                };

                newUser.Settings = new UserSettings
                {
                    Theme = "Dark",
                    MasterVolume = 1.0,
                    User = newUser
                };

                _userRepository.Add(newUser);
                _userRepository.Save();

                MessageBox.Show("Реєстрація успішна! Тепер ви можете увійти.");
                SwitchMode_Click(this, new RoutedEventArgs());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private void SwitchMode_Click(object sender, RoutedEventArgs e)
        {
            _isLoginMode = !_isLoginMode;

            if (_isLoginMode)
            {
                txtTitle.Text = "Вхід";
                btnMain.Content = "Увійти";
                btnSwitch.Content = "Немає акаунту? Реєстрація";
            }
            else
            {
                txtTitle.Text = "Реєстрація";
                btnMain.Content = "Створити акаунт";
                btnSwitch.Content = "Вже є акаунт? Увійти";
            }
        }
    }
}