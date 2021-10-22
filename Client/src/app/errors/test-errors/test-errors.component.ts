import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/User';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {
  baseurl = 'https://localhost:5001/api/';
  validationErrors: string[] | undefined;

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }
  get404Error() {
    this.http.get(this.baseurl + 'Buggy').subscribe(res => {
      console.log(res);
    },
      err => console.log(err)
    )

  }
  get400Error() {
    this.http.get(this.baseurl + 'buggy/bad-request').subscribe(res => {
      console.log(res);
    },
      err => console.log(err)
    )

  }
  get500Error() {
    this.http.get(this.baseurl + 'buggy/server-error').subscribe(res => {
      console.log(res);
    },
      err => console.log(err)
    )

  }
  get401Error() {
    this.http.get(this.baseurl + 'buggy/auth').subscribe(res => {
      console.log(res);
    },
      err => console.log(err)
    )

  }
  get400ValidatonError() {
    this.http.post<User>(this.baseurl + 'account/register', {}).subscribe(res => {
      console.log(res);
    },
      err => {
        console.log(err)
        this.validationErrors = err
      }
    )

  }
}
