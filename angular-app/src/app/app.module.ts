import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { HttpClientModule } from '@angular/common/http';
import { HomeComponent } from './home/home.component';
import { ReactiveFormsModule } from '@angular/forms';
import { TimetableComponent } from './timetable/timetable.component';
import { LoginComponent } from './login/login.component';

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { TokenInterceptor } from './token.interceptor';
import { RegisterComponent } from './register/register.component';
import { LeftMenuComponent } from './left-menu/left-menu.component';
import { RightMenuComponent } from './right-menu/right-menu.component';
import { TicketComponent } from './ticket/ticket.component';

import {MatExpansionModule} from '@angular/material/expansion';
import { MatInputModule, MatButtonModule, MatSelectModule, MatIconModule, MatNativeDateModule, MatDatepickerModule, MatTableModule, MatSortModule, MatFormFieldModule, MatRippleModule, MatCheckboxModule } from '@angular/material';
import { FormsModule } from '@angular/forms';
import { ProfileComponent } from './profile/profile.component';
import { EditUserComponent } from './edit-user/edit-user.component';
import { VerifyUserComponent } from './verify-user/verify-user.component';
import { CheckTicketComponent } from './check-ticket/check-ticket.component';
import { BoughtTicketsComponent } from './bought-tickets/bought-tickets.component';
import { EditLineComponent } from './edit-line/edit-line.component';
import { EditTimetableComponent } from './edit-timetable/edit-timetable.component';
import { EditPricelistComponent } from './edit-pricelist/edit-pricelist.component';
import { EditBusStopComponent } from './edit-bus-stop/edit-bus-stop.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    TimetableComponent,
    LoginComponent,
    RegisterComponent,
    LeftMenuComponent,
    RightMenuComponent,
    TicketComponent,
    ProfileComponent,
    EditUserComponent,
    VerifyUserComponent,
    CheckTicketComponent,
    BoughtTicketsComponent,
    EditLineComponent,
    EditTimetableComponent,
    EditPricelistComponent,
    EditBusStopComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    FormsModule,    
    MatInputModule, 
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTableModule,
    MatSortModule,
    MatExpansionModule,
    MatCheckboxModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
