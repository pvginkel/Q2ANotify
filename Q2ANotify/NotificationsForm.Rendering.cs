using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GdiPresentation;
using Q2ANotify.Database;
using Q2ANotify.Properties;
using Q2ANotify.Support;

namespace Q2ANotify
{
    partial class NotificationsForm
    {
        private const int MaxNotifications = 7;

        private static readonly System.Drawing.Size IconSize = new System.Drawing.Size(40, 40);
        private static readonly ScaledImage IconQuestion = new ScaledImage(Resources.icon_question, IconSize);
        private static readonly ScaledImage IconAnswer = new ScaledImage(Resources.icon_answer, IconSize);
        private static readonly ScaledImage IconComment = new ScaledImage(Resources.icon_comment, IconSize);
        private static readonly ScaledImage IconBadge = new ScaledImage(Resources.icon_badge, IconSize);
        private static readonly ScaledImage IconVote = new ScaledImage(Resources.icon_vote, IconSize);
        private static readonly ScaledImage IconClose = new ScaledImage(Resources.icon_close, new System.Drawing.Size(7, 7));
        private static readonly ScaledImage IconDismiss = new ScaledImage(Resources.icon_dismiss, new System.Drawing.Size(16, 16));
        private static readonly ScaledImage IconMore = new ScaledImage(Resources.icon_more, new System.Drawing.Size(16, 4));

        private Element BuildContent()
        {
            var content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    BuildUserContent(),
                    BuildNotificationsContent()
                }
            };

            return new Border
            {
                BorderThickness = new Thickness(Scale(1)),
                BorderBrush = Brush.Gray,
                Background = new SolidBrush((Color)System.Drawing.SystemColors.Control),
                Padding = new Thickness(Scale(10)),
                Content = content
            };
        }

        private Element BuildUserContent()
        {
            var userName = new TextBlock(_user.Name)
            {
                FontStyle = FontStyle.Bold
            };

            var userPoints = new TextBlock(_user.Points.ToString())
            {
                FontStyle = FontStyle.Bold,
                Margin = new Thickness(Scale(7), 0)
            };

            var userBadges = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            foreach (var badge in _user.Badges)
            {
                Color color;

                switch (badge.Type)
                {
                    case "bronze":
                        color = new Color(203, 145, 20);
                        break;
                    case "silver":
                        color = new Color(205, 205, 205);
                        break;
                    case "gold":
                        color = new Color(255, 204, 1);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                userBadges.Children.Add(new Circle
                {
                    Brush = new SolidBrush(color),
                    Height = Scale(10),
                    Width = Scale(10),
                    VerticalAlignment = VerticalAlignment.Middle,
                    Margin = new Thickness(Scale(4), 0)
                });
                userBadges.Children.Add(new TextBlock(badge.Count.ToString()));
            }

            var dismiss = new Button
            {
                Margin = new Thickness(Scale(7), 0, 0, 0),
                Content = new Image
                {
                    Bitmap = IconDismiss.GetScaled(_dpi)
                }
            };

            dismiss.Click += (s, e) => DismissAll();

            Grid.SetColumn(userPoints, 1);
            Grid.SetColumn(userBadges, 2);
            Grid.SetColumn(dismiss, 3);

            return new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto)
                },
                ColumnDefinitions =
                {
                    // User.
                    new ColumnDefinition(new GridLength(GridUnitType.Star, 1)),
                    // Points.
                    new ColumnDefinition(GridLength.Auto),
                    // Badges.
                    new ColumnDefinition(GridLength.Auto),
                    // Dismiss.
                    new ColumnDefinition(GridLength.Auto)
                },
                Children =
                {
                    userName,
                    userPoints,
                    userBadges,
                    dismiss
                }
            };
        }

        private Element BuildNotificationsContent()
        {
            if (_notifications.Count == 0)
                return new Border();

            var container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, Scale(6), 0, 0)
            };

            for (var i = 0; i < Math.Min(_notifications.Count, MaxNotifications); i++)
            {
                var notification = _notifications[i];
                var image = new Border
                {
                    BorderThickness = new Thickness(0, 0, Scale(1), 0),
                    BorderBrush = SolidBrush.LightGray,
                    Content = new Image
                    {
                        Bitmap = GetImage(notification.Kind),
                        Stretch = Stretch.UniformToFill,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(6)
                    }
                };

                var text = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(Scale(6), Scale(4)),
                    Children =
                    {
                        new TextBlock(notification.Title)
                    }
                };

                if (notification.Message != null)
                    text.Children.Add(new TextBlock(notification.Message));

                if (notification.Poster != null)
                {
                    text.Children.Add(new TextBlock($"By {notification.Poster}")
                    {
                        ForeColor = Color.Gray
                    });
                }

                var close = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Cursor = GdiPresentation.Cursor.Default,
                    Content = new Image
                    {
                        Margin = new Thickness(Scale(4)),
                        Bitmap = IconClose.GetScaled(_dpi)
                    }
                };

                Grid.SetColumn(text, 1);
                Grid.SetColumn(close, 1);

                var content = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition(GridLength.Auto)
                    },
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(new GridLength(GridUnitType.Star, 1))
                    },
                    Children =
                    {
                        image,
                        text,
                        close
                    },
                };

                if (notification.ParentId.HasValue)
                {
                    content.Cursor = GdiPresentation.Cursor.Hand;
                    content.Background = SolidBrush.Transparent;
                    content.MouseUp += (s, e) => _synchronizer.OpenPost(notification.ParentId.Value);
                }

                var shadowColor = Color.LightGray;

                var notificationElement = new ShadowBorder
                {
                    Margin = new Thickness(0, Scale(2)),
                    BorderStartColor = new Color(0, shadowColor.R, shadowColor.G, shadowColor.B),
                    BorderEndColor = shadowColor,
                    BorderWidth = Scale(2),
                    Content = new Border
                    {
                        Background = Brush.White,
                        Content = content
                    }
                };

                long id = notification.Id.Value;
                close.Click += (s, e) => CloseNotification(id);

                container.Children.Add(notificationElement);
            }

            if (_notifications.Count > MaxNotifications)
            {
                container.Children.Add(new Image
                {
                    Bitmap = IconMore.GetScaled(_dpi),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(Scale(4), 0)
                });
            }

            return container;
        }

        private System.Drawing.Bitmap GetImage(string kind)
        {
            switch (kind)
            {
                case "q_post":
                    return IconQuestion.GetScaled(_dpi);
                case "a_post":
                    return IconAnswer.GetScaled(_dpi);
                case "c_post":
                    return IconComment.GetScaled(_dpi);
                case "badge_awarded":
                    return IconBadge.GetScaled(_dpi);
                case "a_select":
                    return IconVote.GetScaled(_dpi);
                default:
                    return null;
            }
        }

        private int Scale(int value)
        {
            return value * _dpi / 96;
        }
    }
}
