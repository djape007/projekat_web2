import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from 'src/models/user';
import { Guid } from 'guid-typescript';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  api_route: String = 'http://localhost:52295';

  constructor(private http: HttpClient) { }

  public register(user: User, file: File): Observable<any>{
    delete user['Dob'];
    delete user['SoldTickets'];
    var formData = new FormData();
    if(file != null)
      formData.append("0",file, file.name);
    for(var key in user)
      formData.append(key, user[key]);
      
    return this.http.post(`${this.api_route}/api/users/register`, formData)
  }

  public uploadPhoto(file: File): Observable<any>{
    var formData = new FormData();
    if(file != null)
      formData.append("0", file, file.name);
      
    return this.http.post(`${this.api_route}/api/users/UploadFiles`, formData)
  }

  public getUser(id: string): Observable<any>{
    return this.http.get(`${this.api_route}/api/Users/${id}`);
  }

  public getProcessingUsers(): Observable<any>{
    return this.http.get(`${this.api_route}/api/Users/ProcessingUsers`);
  }

  public editUser(user: User, dobString: string): Observable<any>{
    return this.http.put(`${this.api_route}/api/Users/${user.Id}`, `Id=${user.Id}&DateOfBirth=${dobString}&Address=${user.Address}&Name=${user.Name}&Surname=${user.Surname}`,  { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}} );
  }

  public changePassword(oldpass: string, newpass: string, reppass: string): Observable<any>{
    return this.http.post(`${this.api_route}/api/Users/ChangePassword`, `OldPassword=${oldpass}&NewPassword=${newpass}&ConfirmPassword=${reppass}`,  { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}});
  }

  public denyUser(userId: Guid): Observable<any>{
    return this.http.put(`${this.api_route}/api/Users/Deny/${userId}`,null);
  }

  public verifyUser(userId: Guid): Observable<any>{
    return this.http.put(`${this.api_route}/api/Users/Verify/${userId}`,null);
  }
}