using Plugin.Contacts;
using Plugin.Contacts.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Diagnostics;

namespace ContactsTest
{
	public class ContactsPage : ContentPage
	{
		
		ListView contactsList;

		public ContactsPage()
		{
			contactsList = new ListView();

			var cell = new DataTemplate(typeof(TextCell));

			cell.SetBinding(TextCell.TextProperty, "FirstName");
			cell.SetBinding(TextCell.DetailProperty, "LastName");

			contactsList.ItemTemplate = cell;

			contactsList.ItemSelected += (sender, args) =>
			{
				if (contactsList.SelectedItem == null)
					return;

				var contact = contactsList.SelectedItem as Contact;

				Navigation.PushAsync(new ContactPage(contact));

				contactsList.SelectedItem = null;
			};

			Content = contactsList;
		}

		bool loaded;

		protected async override void OnAppearing()
		{
			base.OnAppearing();

			if (loaded)
				return;

			loaded = true;



			if (await CrossContacts.Current.RequestPermission())
			{
				this.Title = "Loading...";
				this.IsBusy = true;
				IOrderedQueryable<Contact> contacts = null;
				CrossContacts.Current.PreferContactAggregation = false;
				long ms1 = 0;
				long ms2 = 0;
				//var contactList = new List<Contact>();
				await Task.Run(async () =>
					{
						var watch = Stopwatch.StartNew();
						try
						{
							if (CrossContacts.Current.Contacts == null)
								return;

							contacts = CrossContacts.Current.Contacts
								.Where(c => !string.IsNullOrWhiteSpace(c.LastName) && (c.Emails.Count > 0 || c.Phones.Count > 0))
								.OrderBy(c => c.LastName);

						}
						catch (Exception ex)
						{
							await DisplayAlert("Error", "Some Error"+ ex, "OK");
						}

						watch.Stop();
						ms1 = watch.ElapsedMilliseconds;
						//This is very fast (around 35 MS)

						watch = Stopwatch.StartNew();
						//foreach (var contact in contacts)
						//   contactList.Add(contact);
						watch.Stop();
						ms2 = watch.ElapsedMilliseconds;
						//Slower, around 6s for 1400 items
						//you can actually bind to just contacts for Xamarin.Forms though.
					});


				contactsList.ItemsSource = contacts;

				if (contacts != null)
					this.Title = "Contacts: " + contacts.Count() + " in " + ms1 + " MS" + " & " + ms2 + " Ms";
				else
					this.Title = "Error";
				this.IsBusy = false;
			}
			else
			{
				await DisplayAlert("Permission", "Permission denied :(", "OK");
			}
		}
	}
	/*
	 * Here we define the pages of each contact displaying the detailed info.
	*/
	public class ContactPage : ContentPage
	{
		public ContactPage(Contact contact)
		{
			var stack = new StackLayout{ Spacing = 10, Padding = 10 };

			stack.Children.Add (new Label
			{
					Text = "Id: " + contact.Id
			});
			stack.Children.Add (new Label
			{
					Text = "DisplayName: " + contact.DisplayName
			});
			stack.Children.Add (new Label
			{
					Text = "FirstName: " + contact.FirstName
			});
			stack.Children.Add (new Label
			{
					Text = "LastName: " + contact.LastName
			});
			//display emails of a contact
//			stack.Children.Add (new Label
//			{
//					Text = "Emails: " + contact.Emails
//			});
			foreach(var email in contact.Emails)
			{
			stack.Children.Add (new Label
			{
					Text = "Address: " + email.Address
			});
			}
//			stack.Children.Add (new Label
//			{
//					Text = "Addresses: " + contact.Addresses
//			});
			foreach (var address in contact.Addresses)
			{
				stack.Children.Add (new Label
				{
						Text = "StreetAddress: " + address.StreetAddress
				});
				stack.Children.Add (new Label
				{
						Text = "City: " + address.City
				});
			}
//			stack.Children.Add (new Label
//			{
//					Text = "Phones: " + contact.Phones
//			});
			foreach (var phone in contact.Phones) 
			{
				stack.Children.Add (new Label
				{
						Text = "Number: " + phone.Number
				});
			}
			Content = new ScrollView 
			{
				Content = stack
			};
		}
	}
}

