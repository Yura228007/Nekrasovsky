package com.nekrasovsky.android.data.model

import com.google.gson.annotations.SerializedName

data class User(
    @SerializedName("id")
    val id: Int = 0,
    
    @SerializedName("login")
    val login: String,
    
    @SerializedName("encryptedPassword")
    val encryptedPassword: String = "",
    
    @SerializedName("name")
    val name: String,
    
    @SerializedName("surname")
    val surname: String,
    
    @SerializedName("email")
    val email: String,
    
    @SerializedName("phone")
    val phone: String = "",
    
    @SerializedName("createdAt")
    val createdAt: String = ""
)

