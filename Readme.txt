uMirror
-------

uMirror keeps your Umbraco instance's published content in sync with any external datasource.

It has been designed and tested to work with large amounts of data and comes with an integrated task scheduler and manager.

uMirror is the perfect tool to migrate Umbraco content from an old version when upgrading and also for importing data from any data source.

How it works
------------

uMirror mirrors any changes in an external data source to Umbraco's published content using an XML representation of the data source, which can either be predefined or generated at execution time.

uMirror is an Umbraco backoffice extension which allows a very easy mapping, step by step, between the content's structure from Umbraco and the external data source's XML representation. 

uMirror works with any standard XML format, the XML file doesn't have to implement a specific schema. In fact, if the external data source has an XML representation it can be used directly. If the data source doesn't have an XML representation uMirror can call a proxy method to create it from the source.