using System;

[System.Serializable]
public class ApiResponse<T>
{
    public bool success;
    public T data;
    public string message;
}

[System.Serializable]
public class SaveGameResponse
{
    public long id;
    public string created_at;
    public string player_name;
    public int fires_extinguished;
    public int duration;
}

[System.Serializable]
public class GameLogResponse
{
    public long id;
    public string created_at;
    public string player_name;
    public int fires_extinguished;
    public int duration;
}

[System.Serializable]
public class LeaderboardEntry
{
    public long id;
    public string player_name;
    public int fires_extinguished;
    public int duration;
    public string created_at;
}

[System.Serializable]
public class SaveGameRequest
{
    public string playerName;
    public int firesExtinguished;
    public int duration;
}

// ===== Auth DTOs =====
[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string username;
    public string password;
}

[System.Serializable]
public class AuthResponse
{
    public string userId;
    public string username;
    public string email;
    public string token;
}

[System.Serializable]
public class UserProfile
{
    public string id;
    public string username;
    public string email;
    public string created_at;
    public string last_login;
}

[System.Serializable]
public class ApiErrorResponse
{
    public bool success;
    public string message;
    public string error;
}