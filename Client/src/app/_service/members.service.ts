import { HttpClient, HttpHeaderResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/User';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
const httpOptions = {
  headers: new HttpHeaders({
    Authorization: 'Bearer ' + JSON.parse(localStorage.getItem('user') + "")?.token
  })
}
@Injectable({
  providedIn: 'root'
})
export class MembersService {
  memberCache = new Map();
  members: Member[] = []
  baseurl = environment.apiUrl;
  user!: User;
  userParams!: UserParams;
  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();
  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParams = new UserParams(user);
    })
  }
  resetUserParams() {
    this.userParams = new UserParams(this.user);
    return this.userParams;

  }
  getUserParams() {
    return this.userParams;
  }
  setUserParams(params: UserParams) {
    this.userParams = params;
  }
  getMembers(userParams: UserParams) {
    const key = Object.values(userParams).join('-');
    if (this.memberCache.has(key))
      return of(this.memberCache.get(key));


    let params = this.getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);


    return this.getPaginatedResult<Member[]>(this.baseurl + 'users', params).pipe(map(res => {
      this.memberCache.set(key, res);
      return res;
    }));
  }

  getMember(username: string) {
    const member = this.members.find(x => x.userName === username)
    if (member) return of(member)
    return this.http.get<Member>(this.baseurl + 'users/' + username, httpOptions)
  }
  updateMember(member: Member) {
    return this.http.put(this.baseurl + 'users', member).pipe(map(() => {
      const index = this.members.indexOf(member)
      this.members[index] = member;
    }))
  }
  setMainPhoto(photoId: number) {
    return this.http.put(this.baseurl + 'users/set-main-photo/' + photoId, {});
  }
  deletePhoto(photoId: number) {
    return this.http.delete(this.baseurl + 'users/delete-photo/' + photoId);
  }

  addLike(username: string) {
    return this.http.post(this.baseurl + 'likes/' + username, {});
  }
  getLikes(predicate: string, pageNo: number, pageSize: number) {
    let params = this.getPaginationHeaders(pageNo, pageSize);
    params = params.append('predicate', predicate);
    return this.getPaginatedResult<Partial<Member[]>>(this.baseurl + 'likes/', params);
  }





  private getPaginatedResult<T>(url: string, params: HttpParams) {
    const paginatedResult: PaginatedResult<T> = new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        //@ts-ignore      
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') !== null) {
          //@ts-ignore      
          paginatedResult.pagination = JSON.parse(response.headers.get("Pagination"));
        }
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(page: number, itemsPerPage: number) {
    let params = new HttpParams();
    //@ts-ignore      
    params = params.append('pageNumber', page.toString());
    //@ts-ignore      
    params = params.append('pageSize', itemsPerPage.toString());

    return params;

  }

}
