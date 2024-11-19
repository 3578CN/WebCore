param (
    [string]$localPath,
    [string]$remotePath = "www.3578.cn",         # 远程路径。
    [string]$sftpHost = "39.105.0.223",          # SFTP 主机地址。
    [string]$sftpUsername = "site",              # SFTP 用户名。
    [string]$sftpPassword = "19840602a",         # SFTP 密码。
    [string]$rootUsername = "root",              # 远程执行 root 用户名。
    [string]$rootPassword = "Lion840503",        # 远程执行 root 密码。
    [string]$command = "bash /var/www/sh/3578sh" # 远程执行的命令。
)

# 加载 WinSCP .NET 程序集。
Add-Type -Path "C:\\Program Files (x86)\\WinSCP\\WinSCPnet.dll"

# 使用获取到的 SSH 主机密钥指纹。
$hostKeyFingerprint = "ssh-ed25519 255 9d:ac:b2:19:5d:9b:03:ef:c4:8a:21:6f:76:c8:7f:83"

# 检查路径是否正确。
if (-Not (Test-Path "C:\\Program Files (x86)\\WinSCP\\WinSCPnet.dll")) {
    Write-Error "WinSCP .NET 程序集未找到。请检查路径是否正确。"
    exit 1
}

# 设置会话选项。
$sessionOptions = New-Object WinSCP.SessionOptions
$sessionOptions.Protocol = [WinSCP.Protocol]::Sftp
$sessionOptions.HostName = $sftpHost
$sessionOptions.UserName = $sftpUsername
$sessionOptions.Password = $sftpPassword
$sessionOptions.SshHostKeyFingerprint = $hostKeyFingerprint

# 打开会话。
$session = New-Object WinSCP.Session
try {
    $session.Open($sessionOptions)

    # 清空远程路径下的所有文件。
    Write-Output "清空远程目录中的所有文件..."
    $session.RemoveFiles("$remotePath/*")

    # 设置传输选项。
    Write-Output "开始上传到服务器..."
    $transferOptions = New-Object WinSCP.TransferOptions
    $transferOptions.TransferMode = [WinSCP.TransferMode]::Binary
    $transferOptions.OverwriteMode = [WinSCP.OverwriteMode]::Overwrite

    # 获取本地文件列表。
    $localFiles = Get-ChildItem -Path $localPath

    foreach ($file in $localFiles) {
        if ($file -is [System.IO.FileInfo] -or $file -is [System.IO.DirectoryInfo]) {
            # 上传单个文件或目录。
            $transferResult = $session.PutFiles($file.FullName, "$remotePath/$($file.Name)", $False, $transferOptions)

            # 检查上传结果。
            $transferResult.Check()

            # 显示文件或目录名称，并标记为已上传。
            Write-Output "$($file.Name)   已上传"
        }
    }

    Write-Output "上传完成，开始准备执行远程脚本..."

    # 关闭当前会话。
    $session.Dispose()

    # 使用 root 用户登录执行远程脚本。
    $sessionOptions.UserName = $rootUsername
    $sessionOptions.Password = $rootPassword

    # 重新打开会话。
    $session = New-Object WinSCP.Session
    $session.Open($sessionOptions)

    # 执行远程脚本。
    $executionResult = $session.ExecuteCommand($command)

    # 远程脚本输出。
    Write-Output "----------------------------------------------------------------------------------------------------"
    Write-Output "$($executionResult.Output)"
    Write-Output "----------------------------------------------------------------------------------------------------"
} catch {
    Write-Error "执行过程中出现异常：$_"
} finally {
    if ($session) {
        $session.Dispose()
    }
}