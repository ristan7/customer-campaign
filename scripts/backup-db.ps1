# Backup MySQL baze iz Docker kontejnera u folder backups/.
# Upotreba: .\scripts\backup-db.ps1
# Restore:  Get-Content backups\<fajl>.sql | docker exec -i campaign-mysql sh -c 'exec mysql -uroot -p"$MYSQL_ROOT_PASSWORD" customer_campaign'

$container = "campaign-mysql"

$running = docker ps --filter "name=^$container$" --format "{{.Names}}"
if ($running -ne $container) {
    Write-Error "Container '$container' is not running. Start it with: docker compose up -d"
    exit 1
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$folder = Join-Path $PSScriptRoot "..\backups"
New-Item -ItemType Directory -Force -Path $folder | Out-Null
$file = Join-Path $folder "customer_campaign-$timestamp.sql"

docker exec $container sh -c 'exec mysqldump --single-transaction --routines -uroot -p"$MYSQL_ROOT_PASSWORD" customer_campaign' |
    Out-File -FilePath $file -Encoding utf8

if ($LASTEXITCODE -ne 0 -or (Get-Item $file).Length -lt 100) {
    Remove-Item $file -ErrorAction SilentlyContinue
    Write-Error "Backup failed."
    exit 1
}

Write-Host "Backup created: $file ($([math]::Round((Get-Item $file).Length / 1KB, 1)) KB)"