package com.nekrasovsky.android.ui.screens.dashboard

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.nekrasovsky.android.data.signalr.SignalRService
import com.nekrasovsky.android.data.sound.AlarmSoundService
import com.nekrasovsky.android.ui.viewmodel.DashboardViewModel
import kotlinx.coroutines.launch

data class DashboardItem(
    val title: String,
    val icon: androidx.compose.ui.graphics.vector.ImageVector,
    val onClick: () -> Unit
)

@Composable
fun DashboardScreen(
    onNavigateToProducts: () -> Unit,
    onNavigateToMaterials: () -> Unit,
    onNavigateToWarehouses: () -> Unit,
    onNavigateToWorkReports: () -> Unit,
    onAlarmClick: () -> Unit,
    onLogout: () -> Unit,
    viewModel: DashboardViewModel = hiltViewModel()
) {
    val scope = rememberCoroutineScope()
    val currentUser by viewModel.currentUser
    val alarmNotification by viewModel.alarmNotification
    var showAlarmNotificationDialog by remember { mutableStateOf(false) }
    var showCreateAlarmDialog by remember { mutableStateOf(false) }
    var alarmLocation by remember { mutableStateOf("") }
    var alarmMessage by remember { mutableStateOf("") }

    // Отслеживаем изменения уведомлений
    LaunchedEffect(alarmNotification) {
        if (alarmNotification != null) {
            showAlarmNotificationDialog = true
        }
    }

    // Диалог получения уведомления о тревоге
    if (showAlarmNotificationDialog && alarmNotification != null) {
        AlertDialog(
            onDismissRequest = { 
                showAlarmNotificationDialog = false
                viewModel.alarmNotification.value = null
            },
            title = { Text("🚨 ТРЕВОГА!", fontWeight = FontWeight.Bold) },
            text = { 
                Column {
                    Text("${alarmNotification!!.first}\n", fontWeight = FontWeight.Bold)
                    Text("Место: ${alarmNotification!!.second}")
                    Text("Пользователь: ${alarmNotification!!.third}")
                }
            },
            confirmButton = {
                TextButton(onClick = { 
                    showAlarmNotificationDialog = false
                    viewModel.alarmNotification.value = null
                }) {
                    Text("OK")
                }
            }
        )
    }

    // Диалог создания тревоги
    if (showCreateAlarmDialog) {
        AlertDialog(
            onDismissRequest = { 
                showCreateAlarmDialog = false
                alarmLocation = ""
                alarmMessage = ""
            },
            title = { Text("🚨 СОЗДАТЬ ТРЕВОГУ", fontWeight = FontWeight.Bold) },
            text = {
                Column(
                    modifier = Modifier.fillMaxWidth(),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    TextField(
                        value = alarmLocation,
                        onValueChange = { alarmLocation = it },
                        label = { Text("Место происшествия *") },
                        modifier = Modifier.fillMaxWidth(),
                        singleLine = true
                    )
                    TextField(
                        value = alarmMessage,
                        onValueChange = { alarmMessage = it },
                        label = { Text("Описание (необязательно)") },
                        modifier = Modifier.fillMaxWidth(),
                        maxLines = 3
                    )
                }
            },
            confirmButton = {
                TextButton(
                    onClick = {
                        if (alarmLocation.isNotBlank() && currentUser != null) {
                            scope.launch {
                                val success = viewModel.createAlarmEvent(
                                    location = alarmLocation,
                                    message = alarmMessage.ifBlank { null }
                                )
                                if (success) {
                                    showCreateAlarmDialog = false
                                    alarmLocation = ""
                                    alarmMessage = ""
                                }
                            }
                        }
                    },
                    enabled = alarmLocation.isNotBlank() && currentUser != null
                ) {
                    Text("Отправить")
                }
            },
            dismissButton = {
                TextButton(onClick = { 
                    showCreateAlarmDialog = false
                    alarmLocation = ""
                    alarmMessage = ""
                }) {
                    Text("Отмена")
                }
            }
        )
    }
    val dashboardItems = listOf(
        DashboardItem(
            title = "Продукты",
            icon = Icons.Default.Inventory,
            onClick = onNavigateToProducts
        ),
        DashboardItem(
            title = "Материалы",
            icon = Icons.Default.Build,
            onClick = onNavigateToMaterials
        ),
        DashboardItem(
            title = "Склады",
            icon = Icons.Default.Storage,
            onClick = onNavigateToWarehouses
        ),
        DashboardItem(
            title = "Отчеты о работе",
            icon = Icons.Default.Assignment,
            onClick = onNavigateToWorkReports
        )
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { 
                    Text(
                        "Главная",
                        fontWeight = FontWeight.Bold
                    ) 
                },
                actions = {
                    IconButton(onClick = onLogout) {
                        Icon(
                            imageVector = Icons.Default.Logout,
                            contentDescription = "Выход"
                        )
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            LazyVerticalGrid(
                columns = GridCells.Fixed(2),
                contentPadding = PaddingValues(16.dp),
                horizontalArrangement = Arrangement.spacedBy(16.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp),
                modifier = Modifier.weight(1f)
            ) {
                items(dashboardItems) { item ->
                    DashboardCard(
                        title = item.title,
                        icon = item.icon,
                        onClick = item.onClick
                    )
                }
            }
            
            // Alarm Button
            Card(
                onClick = { showCreateAlarmDialog = true },
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp)
                    .height(80.dp),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.error
                ),
                elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
            ) {
                Row(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(20.dp),
                    horizontalArrangement = Arrangement.Center,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = "🚨 ТРЕВОГА",
                        style = MaterialTheme.typography.headlineMedium,
                        fontWeight = FontWeight.Bold,
                        color = MaterialTheme.colorScheme.onError
                    )
                }
            }
        }
    }
}

@Composable
fun DashboardCard(
    title: String,
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    onClick: () -> Unit
) {
    Card(
        onClick = onClick,
        modifier = Modifier
            .fillMaxWidth()
            .height(150.dp),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        ),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(20.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Icon(
                imageVector = icon,
                contentDescription = title,
                modifier = Modifier.size(48.dp),
                tint = MaterialTheme.colorScheme.primary
            )
            Spacer(modifier = Modifier.height(12.dp))
            Text(
                text = title,
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.SemiBold,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}

