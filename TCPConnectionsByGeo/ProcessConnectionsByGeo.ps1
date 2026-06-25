$connections = Get-NetTCPConnection -State Established |
    Where-Object {
        $_.RemoteAddress -notmatch '^(127\.|0\.0\.0\.0|::1|fe80:)' -and
        $_.RemoteAddress -notlike '192.168.*' -and
        $_.RemoteAddress -notlike '10.*'
    }

$cache = @{}

$results = foreach ($conn in $connections) {

    $ip = $conn.RemoteAddress

    if (-not $cache.ContainsKey($ip)) {
        try {
            $geo = Invoke-RestMethod -Uri "http://ip-api.com/json/$ip" -TimeoutSec 5
            $cache[$ip] = $geo
        }
        catch {
            $cache[$ip] = $null
        }
    }

    $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
    $geo = $cache[$ip]

    [PSCustomObject]@{
        Process      = $proc.ProcessName
        PID          = $conn.OwningProcess
        LocalAddress = $conn.LocalAddress
        LocalPort    = $conn.LocalPort
        RemoteIP     = $ip
        RemotePort   = $conn.RemotePort
        State        = $conn.State

        Country      = $geo.country
        Region       = $geo.regionName
        City         = $geo.city
        Latitude     = $geo.lat
        Longitude    = $geo.lon
        ISP          = $geo.isp
    }
}

$results | Format-Table -AutoSize