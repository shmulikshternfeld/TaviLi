echo Seeding Database...
:: Try HTTP
curl -X POST http://localhost:5193/api/testdata/seed
:: Try HTTPS as fallback (ignoring certs)
curl -k -X POST https://localhost:7298/api/testdata/seed
echo.
echo If success message appeared above, you are good to go!
pause
