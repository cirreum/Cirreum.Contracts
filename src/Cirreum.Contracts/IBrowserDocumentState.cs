namespace Cirreum;

using Cirreum.State;

/// <summary>
/// Defines state and display settings for the browser document hosting the application.
/// </summary>
/// <remarks>
/// Manages browser-level presentation settings, including the document title,
/// application name, and progressive web application display mode.
/// </remarks>
public interface IBrowserDocumentState : IScopedNotificationState {

	/// <summary>
	/// Gets the name of the application.
	/// </summary>
	string AppName { get; }

	/// <summary>
	/// Sets the <see cref="AppName"/> for the application.
	/// </summary>
	/// <param name="value">The application name.</param>
	void SetAppName(string value);

	/// <summary>
	/// Gets the default prefix applied to browser document titles.
	/// </summary>
	/// <remarks>
	/// When specified, this value appears before the primary document title
	/// and is separated from it by <see cref="DocumentTitleSeparator"/>.
	/// </remarks>
	string DocumentTitlePrefix { get; }

	/// <summary>
	/// Sets the <see cref="DocumentTitlePrefix"/> applied to browser document titles.
	/// </summary>
	/// <param name="value">
	/// The text to place before the primary document title.
	/// </param>
	void SetDocumentTitlePrefix(string value);

	/// <summary>
	/// Gets the default suffix applied to browser document titles.
	/// </summary>
	/// <remarks>
	/// When specified, this value appears after the primary document title
	/// and is separated from it by <see cref="DocumentTitleSeparator"/>.
	/// This value commonly contains the application name.
	/// </remarks>
	string DocumentTitleSuffix { get; }

	/// <summary>
	/// Sets the <see cref="DocumentTitleSuffix"/> applied to browser document titles.
	/// </summary>
	/// <param name="value">
	/// The text to place after the primary document title.
	/// </param>
	void SetDocumentTitleSuffix(string value);

	/// <summary>
	/// Gets the separator used between the primary document title and its
	/// configured prefix or suffix.
	/// </summary>
	/// <remarks>
	/// Common values include <c>|</c>, <c>-</c>, and <c>•</c>.
	/// </remarks>
	string DocumentTitleSeparator { get; }

	/// <summary>
	/// Sets the <see cref="DocumentTitleSeparator"/> used when composing browser
	/// document titles.
	/// </summary>
	/// <param name="value">
	/// The text used to separate the primary document title from its configured
	/// prefix or suffix.
	/// </param>
	void SetDocumentTitleSeparator(string value);

	/// <summary>
	/// Gets a value indicating whether the application is running in standalone
	/// display mode.
	/// </summary>
	/// <remarks>
	/// This value should reflect the browser's <c>display-mode: standalone</c>
	/// media query. Standalone mode commonly indicates that the application was
	/// launched as an installed Progressive Web Application.
	/// </remarks>
	bool IsStandAlone { get; }

	/// <summary>
	/// Sets whether the application is running in standalone display mode.
	/// </summary>
	/// <param name="value">
	/// <see langword="true"/> when the application is running in standalone
	/// display mode; otherwise, <see langword="false"/>.
	/// </param>
	/// <remarks>
	/// This value may be used to adjust document titles or enable presentation
	/// behavior specific to installed Progressive Web Applications.
	/// </remarks>
	void SetIsStandAlone(bool value);

}