# Q2A Notify

LGPL License.

[Download Setup](https://github.com/pvginkel/Q2ANotify/raw/master/Support/Q2ANotify%20Setup.exe).

## Introduction

Q2A Notify is a Windows desktop notification application for the
[Question2Answer](http://www.question2answer.org/) application.

This application allows you to get updated on new questions,
answers and comments to questions you've participated in and badge updates.

![Create shortcut](https://github.com/pvginkel/Q2ANotify/raw/master/Support/Popup.png)

## Installation

Q2A Notify requires two applications to be installed:

* [q2a-notify-plugin](https://github.com/pvginkel/q2a-notify-plugin): Adds a notification feed
  to Question2Answer the plugin uses to retrieve updates;
* [q2a-badges](https://github.com/pvginkel/q2a-badges): Adds support for badges.

### q2a-badges

The badges plugin at the moment is required. I may change the application to detect whether the
plugin is installed and not show badges if not, but at the moment this is not supported.

The version of q2a-badges from [heliochun](https://github.com/heliochun/q2a-badges) at the moment
is not supported. The reason for this is that the default version of q2a-badges does not
alert of badge updates through the event API. This is required for Q2A Notify to be
able to send notifications of earned badges. A PR has been made available to
[heliochun](https://github.com/heliochun/q2a-badges) at
[Send badge awarded events through the event system](https://github.com/heliochun/q2a-badges/pull/1).

The plugin won't break if another version of q2a-badges is installed. The only difference is
that you won't receive notifications of earned badges. The badge count will still be shown.