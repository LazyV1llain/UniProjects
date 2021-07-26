using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class Splash : Form
    {
        /// <summary>
        /// Конструктор формы.
        /// </summary>
        public Splash()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Метод активации таймера при загруке формы.
        /// </summary>
        /// <param name="sender">Объект, сгенерировавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void Splash_Load(object sender, EventArgs e)
        {
            // Активация таймера.
            SplashTimer.Enabled = true;
        }

        /// <summary>
        /// Обработчик события "тика" таймера.
        /// </summary>
        /// <param name="sender">Объект, сгенерировавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void SplashTimer_Tick(object sender, EventArgs e)
        {
            // Закрытие формы.
            Close();
        }
    }
}
