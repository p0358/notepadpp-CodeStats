
History
-------


1.1.1 (2022-11-01)
++++++++++++++++++

- Fixed version check message claiming your version is outdated if it was in fact newer (it was checking for simple equality)
- Pulses that were not possible to be sent (for example due to connection issues) will now be dumped to disk on Notepad++ close and later restored, instead of being discarded


1.1.0 (2022-10-20)
++++++++++++++++++

- Fixed SSL/TLS errors by allowing the use of TLS 1.3, 1.2 and 1.1, plus disallowing use of the older ones
- Fixed stats URL being broken
- Supports editor's selected language for language detection, adds priority settings for detection
- Supports creating custom extension mapping JSON, whose values take precedence over built-in ones
- Fixed "Access violation" exception on exit if no pulses were made before the editor was closed
- Made logging asynchronous, so its I/O operations never block the current thread
- Probably various other fixes I forgot about


1.0.1 (2017-12-30)
++++++++++++++++++

- Added the ability to change API URL in GUI
- Added user-agent for API pulses
- Some bug fixes


1.0.0 (2017-12-30)
++++++++++++++++++

- Initial version

