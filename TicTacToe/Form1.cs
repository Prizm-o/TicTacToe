using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;

namespace TicTacToe
{
    public partial class Form1 : Form
    {
        private HubConnection connection;

        public Form1()
        {
            InitializeComponent();
            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7127/gamehub")
                .Build();
            
            connection.Closed += async (error) =>
            {
                System.Threading.Thread.Sleep(5000);
                await connection.StartAsync();
            };
            //this.Load += new System.EventHandler(this.Form1_Load);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Привязка данных к кнопкам.
            for (int i = 0; i < 9; i++)
            {
                this.Controls["btnCell" + i].BackColor = SystemColors.ScrollBar;
                this.Controls["btnCell" + i].Enabled = false;
                this.Controls["btnCell" + i].Click += btnCell_Click;
            };

            try
            {
                await connection.StartAsync();
                listBox1.Items.Add(connection.State);
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(connection.State);
                listBox1.Items.Add(ex.Message);
            }
            connection.On<string>("ReceiveMessage", message =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    listBox1.Items.Add(message);
                });
                
            });

            connection.On("StartGame", () =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    listBox1.Items.Add("Сделайте ход");

                    for (int i = 0; i < 9; i++)
                    {
                        this.Controls["btnCell" + i].BackColor = SystemColors.Window;
                        this.Controls["btnCell" + i].Enabled = true;
                        this.Controls["btnCell" + i].Text = "";
                    };
                    
                });
            });

            connection.On("JoinGame", () =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    textBox1.Enabled = false;
                    this.Controls["SendBtn"].Text = "Выйти";
                    this.Controls["SendBtn"].Click -= SendBtn_Click;
                    this.Controls["SendBtn"].Click += ExitBtn_Click;                    
                });
            });

            connection.On("ExitGame", () =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    textBox1.Enabled = true;
                    this.Controls["SendBtn"].Text = "Присоединиться";
                    this.Controls["SendBtn"].Click -= ExitBtn_Click;
                    this.Controls["SendBtn"].Click += SendBtn_Click;
                });
            });

            connection.On<string>("UpdateScore", (message) =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    labelScore.Text = message;
                });
            });

            connection.On<string[]>("UpdateBoard", (board) =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    UpdateGameBoard(board);
                });
            });

            connection.On<string[]>("UpdateBoardViewers", (board) =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    UpdateBoardViewers(board);
                });
            });

            connection.On("NextMove", () =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    NextMove();
                });
            });

            connection.On<string>("GameOver", message =>
            {
                Dispatcher.Invoke(this, () =>
                {
                    listBox1.Items.Add(message);
                    EndGame();
                });
            });
        }

        private async void btnCell_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int x = Convert.ToInt32(button.Tag.ToString());
            
            await connection.InvokeAsync("MakeMove", x, connection.ConnectionId);
        }

        private void NextMove() //Блокируем кнопки у того, кто ходил
        {
            for (int i = 0; i < 9; i++)
            {
                Button button = this.Controls.Find($"btnCell{i}", true)[0] as Button;
                button.Enabled = false;
                if (!button.Enabled)
                {
                    if (button.Text != "")
                    {
                        button.BackColor = SystemColors.InactiveCaption;
                    }
                    else
                    {
                        button.BackColor = SystemColors.ScrollBar;
                    }
                }
            }
        }

        private void UpdateGameBoard(string[] board) //Обновляем информацию в клетках для игроков
        {
            for (int i = 0; i < 9; i++)
            {
                Button button = this.Controls.Find($"btnCell{i}", true)[0] as Button;
                button.Text = board[i] ?? "";
                button.Enabled = board[i] == null; // Блокируем кнопки после хода
                if (!button.Enabled) 
                {
                    if (button.Text != "")
                    {
                        button.BackColor = SystemColors.InactiveCaption;
                    }
                    else
                    {
                        button.BackColor = SystemColors.ScrollBar;
                    }
                } else { button.BackColor = SystemColors.Window; }
            }
        }

        private void UpdateBoardViewers(string[] board) //Обновляем информацию в клетках для смотрящих
        {
            // Обновите кнопки на форме в зависимости от состояния игрового поля
            for (int i = 0; i < 9; i++)
            {
                Button button = this.Controls.Find($"btnCell{i}", true)[0] as Button;
                button.Text = board[i] ?? "";
                button.Enabled = false; // Запрещаем снова нажимать на кнопки
                if (!button.Enabled)
                {
                    if (button.Text != "")
                    {
                        button.BackColor = SystemColors.InactiveCaption;
                    }
                    else
                    {
                        button.BackColor = SystemColors.ScrollBar;
                    }
                }
                else { button.BackColor = SystemColors.Window; }
            }
        }

        private void EndGame()
        {
            for (int i = 0; i < 9; i++)
            {
                Button button = this.Controls.Find($"btnCell{i}", true)[0] as Button;
                button.Text = "";
                button.Enabled = false; 
                button.BackColor = SystemColors.ScrollBar;                
            }
        }

        private async void SendBtn_Click(object sender, EventArgs e)
        {
            try
            {
                await connection.InvokeAsync("Connect", textBox1.Text, connection.ConnectionId);
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(connection.State);
                listBox1.Items.Add(ex.Message);
            }
        }

        private async void ExitBtn_Click(object sender, EventArgs e)
        {
            try
            {
                await connection.InvokeAsync("Disconnect", connection.ConnectionId);
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(connection.State);
                listBox1.Items.Add(ex.Message);
            }
        }

        //Dispatcher для отключение элементов от потока
        public delegate void AsyncAction();

        public delegate void DispatcherInvoker(Form form, AsyncAction a);

        public class Dispatcher
        {
            public static void Invoke(Form form, AsyncAction action)
            {
                if (!form.InvokeRequired)
                {
                    action();
                }
                else
                {
                    form.Invoke((DispatcherInvoker)Invoke, form, action);
                }
            }
        }
    }
}
