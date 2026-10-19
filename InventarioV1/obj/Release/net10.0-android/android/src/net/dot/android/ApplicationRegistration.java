package net.dot.android;

public class ApplicationRegistration {

	public static android.content.Context Context;

	public static void registerApplications ()
	{
		// Application and Instrumentation ACWs must be registered first.
		mono.android.Runtime.register ("Microsoft.Maui.MauiApplication, Microsoft.Maui, Version=10.0.0.0, Culture=neutral, PublicKeyToken=null", crc6488302ad6e9e4df1a.MauiApplication.class, crc6488302ad6e9e4df1a.MauiApplication.__md_methods);
		mono.android.Runtime.register ("InventarioV1.MainApplication, InventarioV1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", crc641d8c0b463b30461b.MainApplication.class, crc641d8c0b463b30461b.MainApplication.__md_methods);
		
	}
}
